using myfoodapp.Hub.Models;
using Newtonsoft.Json;
using SimpleExpressionEvaluator;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace myfoodapp.Hub.Business
{
    public class AquaponicsRulesManager
    {
        public static bool ValidateRules(GroupedMeasure currentMeasures, int productionUnitId)
        {
            Evaluator evaluator = new Evaluator();
            bool isValid = true;

            ApplicationDbContext db = new ApplicationDbContext();
            ApplicationDbContext dbLog = new ApplicationDbContext();

            var data = File.ReadAllText(HostingEnvironment.MapPath("~/Content/SmartGreenhouseRules.json"));
            var rulesList = JsonConvert.DeserializeObject<List<Rule>>(data);

            var currentProductionUnit = db.ProductionUnits.Include(p => p.owner.language).Where(p => p.Id == productionUnitId).FirstOrDefault();
            var warningEventType = db.EventTypes.Where(p => p.Id == 1).FirstOrDefault();

            var currentProductionUnitOwner = currentProductionUnit.owner;

            foreach (var rule in rulesList)
            {
                var warningContent =String.Empty;
                bool rslt = false;

                try
                {
                    try
                    {
                        rslt = evaluator.Evaluate(rule.ruleEvaluator, currentMeasures);
                        warningContent = rule.warningContent;
                    }
                    catch (Exception ex)
                    {
                        dbLog.Logs.Add(Log.CreateErrorLog(String.Format("Error with Rule Manager Evaluator - {0}", rule.ruleEvaluator), ex));
                        dbLog.SaveChanges();
                    }

                    if (currentProductionUnitOwner != null && currentProductionUnitOwner.language != null)
                    {
                        switch (currentProductionUnitOwner.language.description)
                        {
                            case "fr":
                                warningContent = rule.warningContentFR;
                                break;
                            default:
                                break;
                        }
                    }

                    var bindingValue = currentMeasures.GetType().GetProperty(rule.bindingPropertyValue).GetValue(currentMeasures, null);
                    var message = String.Format(warningContent, bindingValue);

                    if (rslt)
                    {
                            if (currentProductionUnit != null)
                            {
                                db.Events.Add(new Event() { date = DateTime.Now, description = message, isOpen = false, productionUnit = currentProductionUnit, eventType = warningEventType, createdBy = "MyFood Bot" });
                                db.SaveChanges();
                            }

                        isValid = false;
                    }
                }
                catch (Exception ex)
                {
                    dbLog.Logs.Add(Log.CreateErrorLog(String.Format("Error with Rule Manager Evaluator - {0}",rule.ruleEvaluator), ex));
                    dbLog.SaveChanges();
                }
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                dbLog.Logs.Add(Log.CreateErrorLog(String.Format("Error with Rule Manager - Save Events"), ex));
                dbLog.SaveChanges();
            }

            return isValid;
        }

        public static GroupedMeasure MeasuresProcessor(int productionUnitId)
        {
            var db = new ApplicationDbContext();
            var dbLog = new ApplicationDbContext();

            ProductionUnit currentProductionUnit = db.ProductionUnits.Include(p => p.hydroponicType)
                                                                     .Where(p => p.Id == productionUnitId).FirstOrDefault();

            var phSensor = db.SensorTypes.Where(s => s.Id == 1).FirstOrDefault();
            var waterTemperatureSensor = db.SensorTypes.Where(s => s.Id == 2).FirstOrDefault();
            var dissolvedOxySensor = db.SensorTypes.Where(s => s.Id == 3).FirstOrDefault();
            var ORPSensor = db.SensorTypes.Where(s => s.Id == 4).FirstOrDefault();
            var airTemperatureSensor = db.SensorTypes.Where(s => s.Id == 5).FirstOrDefault();
            var airHumidity = db.SensorTypes.Where(s => s.Id == 6).FirstOrDefault();

            DateTime thisDay = DateTime.Now;
            DateTime lastDay = thisDay.AddDays(-1);
            DateTime twoDaysAgo = thisDay.AddDays(-2);
            DateTime threeDaysAgo = thisDay.AddDays(-3);
            DateTime aWeekAgo = thisDay.AddDays(-7);

            GroupedMeasure currentMeasures = new GroupedMeasure();
            currentMeasures.hydroponicTypeName = currentProductionUnit.hydroponicType.name;

            try
            {
                var currentLastDayPHValueMax = db.Measures.Where(m => m.captureDate > lastDay &&
                                             m.productionUnit.Id == currentProductionUnit.Id &&
                                             m.sensor.Id == phSensor.Id).OrderBy(m => m.Id).Max(t => t.value);

                var currentLastDayPHValueMin = db.Measures.Where(m => m.captureDate > lastDay &&
                                   m.productionUnit.Id == currentProductionUnit.Id &&
                                   m.sensor.Id == phSensor.Id).OrderBy(m => m.Id).Min(t => t.value);

                var currentTwoDaysPHValueMax = db.Measures.Where(m => m.captureDate > twoDaysAgo && m.captureDate < lastDay &&
                                   m.productionUnit.Id == currentProductionUnit.Id &&
                                   m.sensor.Id == phSensor.Id).OrderBy(m => m.Id).Max(t => t.value);

                var currentTwoDaysPHValueMin = db.Measures.Where(m => m.captureDate > twoDaysAgo && m.captureDate < lastDay &&
                                   m.productionUnit.Id == currentProductionUnit.Id &&
                                   m.sensor.Id == phSensor.Id).OrderBy(m => m.Id).Min(t => t.value);

                var currentThreeDaysPHValueMax = db.Measures.Where(m => m.captureDate > threeDaysAgo && m.captureDate < twoDaysAgo &&
                                   m.productionUnit.Id == currentProductionUnit.Id &&
                                   m.sensor.Id == phSensor.Id).OrderBy(m => m.Id).Max(t => t.value);

                var currentThreeDaysPHValueMin = db.Measures.Where(m => m.captureDate > threeDaysAgo && m.captureDate < twoDaysAgo &&
                                   m.productionUnit.Id == currentProductionUnit.Id &&
                                   m.sensor.Id == phSensor.Id).OrderBy(m => m.Id).Min(t => t.value);

                var currentLastWeekPHValueMean = db.Measures.Where(m => m.captureDate > aWeekAgo &&
                   m.productionUnit.Id == currentProductionUnit.Id &&
                   m.sensor.Id == phSensor.Id).OrderBy(m => m.Id).Average(t => t.value);

                currentMeasures.lastDayPHvariation = Math.Round(Math.Abs(currentLastDayPHValueMax - currentLastDayPHValueMin), 1);
                currentMeasures.threeLastDayPHvariation = Math.Round((Math.Abs(currentLastDayPHValueMax - currentLastDayPHValueMin) + Math.Abs(currentTwoDaysPHValueMax - currentTwoDaysPHValueMin) + Math.Abs(currentThreeDaysPHValueMax - currentThreeDaysPHValueMin)) / 3, 1);
                currentMeasures.lastWeekPHmean = Math.Round(currentLastWeekPHValueMean, 1);

                var currentTwoDaysAirTempValueMax = db.Measures.Where(m => m.captureDate > lastDay &&
                                   m.productionUnit.Id == currentProductionUnit.Id &&
                                   m.sensor.Id == airTemperatureSensor.Id).OrderBy(m => m.Id).Max(t => t.value);

                var currentLastDayAirTempValueMin = db.Measures.Where(m => m.captureDate > lastDay &&
                                   m.productionUnit.Id == currentProductionUnit.Id &&
                                   m.sensor.Id == airTemperatureSensor.Id).OrderBy(m => m.Id).Min(t => t.value);

                var currentLastDayAirTempValueMean = db.Measures.Where(m => m.captureDate > lastDay &&
                   m.productionUnit.Id == currentProductionUnit.Id &&
                   m.sensor.Id == airTemperatureSensor.Id).OrderBy(m => m.Id).Sum(t => t.value) / (6 * 24);

                currentMeasures.lastDayMaxAirTempvalue = Math.Round(currentTwoDaysAirTempValueMax, 1);
                currentMeasures.lastDayMinAirTempvalue = Math.Round(currentLastDayAirTempValueMin, 1);
                currentMeasures.lastDayMeanAirTempvalue = Math.Round(currentLastDayAirTempValueMean, 1);

                var currentTwoDaysWaterTempValueMax = db.Measures.Where(m => m.captureDate > lastDay &&
                   m.productionUnit.Id == currentProductionUnit.Id &&
                   m.sensor.Id == waterTemperatureSensor.Id).OrderBy(m => m.Id).Max(t => t.value);

                var currentLastDayWaterTempValueMin = db.Measures.Where(m => m.captureDate > lastDay &&
                                   m.productionUnit.Id == currentProductionUnit.Id &&
                                   m.sensor.Id == waterTemperatureSensor.Id).OrderBy(m => m.Id).Min(t => t.value);

                currentMeasures.lastDayMaxWaterTempvalue = Math.Round(currentTwoDaysWaterTempValueMax, 1);
                currentMeasures.lastDayMinWaterTempvalue = Math.Round(currentLastDayWaterTempValueMin, 1);

                var currentTwoDaysHumidityValueMax = db.Measures.Where(m => m.captureDate > lastDay &&
                                   m.productionUnit.Id == currentProductionUnit.Id &&
                                   m.sensor.Id == airHumidity.Id).OrderBy(m => m.Id).Max(t => t.value);

                currentMeasures.lastDayMaxHumidityvalue = Math.Round(currentTwoDaysHumidityValueMax, 1);
            }
            catch (Exception ex)
            {
                dbLog.Logs.Add(Log.CreateErrorLog("Error on Measures Processing", ex));
                dbLog.SaveChanges();
            }     

            return currentMeasures;
        }
    }

    public class GroupedMeasure
    {
        public Int64 Id { get; set; }
        public DateTime captureDate { get; set; }
        public decimal pHvalue { get; set; }
        public decimal lastDayMinpHvalue { get; set; }
        public decimal lastDayMaxpHvalue { get; set; }
        public decimal lastDayPHvariation { get; set; }
        public decimal threeLastDayPHvariation { get; set; }
        public decimal lastWeekPHmean { get; set; }
        public decimal waterTempvalue { get; set; }
        public decimal lastDayMinWaterTempvalue { get; set; }
        public decimal lastDayMaxWaterTempvalue { get; set; }
        public decimal ORPvalue { get; set; }
        public decimal DOvalue { get; set; }
        public decimal airTempvalue { get; set; }
        public decimal lastDayMinAirTempvalue { get; set; }
        public decimal lastDayMaxAirTempvalue { get; set; }
        public decimal lastDayMeanAirTempvalue { get; set; }
        public decimal humidityvalue { get; set; }
        public decimal lastDayMaxHumidityvalue { get; set; }
        public string hydroponicTypeName { get; set; }
    }
}