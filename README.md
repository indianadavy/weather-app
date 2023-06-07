# Weather App

## Description

This is a simple weather API that uses the `open-meteo` API to display the current weather and a 5-day forecast for a given city. The app also saves the user's search history and allows them to click on a city to see the weather for that city again.

## requirements

- [Docker](https://docs.docker.com/get-docker/)

## Running the app

```shell
docker build -t my-mongo-image .
docker run --name my-mongo-container -p 27017:27017 -d my-mongo-image

```

## Querying MongoDB

Open a terminal and run the following command to open a mongo shell:

```shell
docker exec -it my-mongo-container mongosh
```

Once in the mongo shell, run the following commands to switch to the `weather` database and see the `forecasts` collection:

```shell
use weather
show collections
```

To see all the documents in the `forecasts` collection, run the following command:

```shell
# show all documents in the forecasts collection
db.forecasts.find().pretty()

# show a specific document in the forecasts collection
db.forecasts.find({ _id: "892634234" })
```

