db = new Mongo().getDB("weather");

db.createCollection("forecasts");

db.forecasts.createIndex({ location: "2dsphere" }, { unique: true });
