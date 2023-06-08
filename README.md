# Weather App

## Description

This is a simple weather API that uses the `open-meteo` API to display the current weather for a given `longitude` and `latitude`. The app also saves the user's search history on a `mongodb` and allows them select a previous coordinate to get the weather again.

## requirements

- [Docker](https://docs.docker.com/get-docker/)

## Running the app

First thing you need to do is start the database. Run the following command to start a MongoDB container:

```shell
docker build -t my-mongo-image .
docker run --name my-mongo-container -p 27017:27017 -d my-mongo-image
```

Next, you need to start the API. By running it in your IDE of choice or run the following commands on the root of the project:

```shell
cd WeatherApp
dotnet run
```

Then you can interact with the API on your browser by going to `http://localhost:5102/swagger/index.html`.

## Running the tests

To run the tests, run the following command this command on a terminal on the root of the project:

```shell
dotnet test
```

## Running the app with docker-compose

To run the app with docker-compose, move to the demo folder and run the following commands:

```shell
cd demo
docker-compose up --build -d
```

This will start the API and the database. You can then interact with the API on your browser by going to `http://localhost:5102/swagger/index.html`.

Alternatively, you can run hit the API directly with Postman or curl. For example:

```shell
curl -X POST -H "Content-Type: application/json" -d '{"latitude": 0, "longitude": 5}' http://localhost:5102/WeatherForecast

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
