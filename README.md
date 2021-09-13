# movie-search-api-v2
movie search engine
asp.net 5 core api with elasticsearch client and liteDB for handling user event updates (clicks/views) and migratinbg it over to elasticsearch when traffic is low to get around  many elasticsearch updates which are very I/O intensive

## acknowledgements
based on [This stackoverflow answer](https://stackoverflow.com/questions/41711961/elasticsearch-user-clicks-feedback/41716811#41716811)

## structure
 - Movie.Base (business logic and service interfaces)
 - Movie.CronJob (cronjob scheduler using Quartz) for updating elastic search on interval
 - Movie.DB (database client interfaces and data access layer) liteDB chosen for simplicity and speed
 - Movie.ElasticSearch (elasticsearch client) using NEST
 - Movie.WebAPI (routes, controllers and service classes)

## usage
start elasticsearch
  ```
  docker run -d -p 9200:9200 -p 5601:5601 -e "discovery.type=single-node" docker.elastic.co/elasticsearch/elasticsearch:7.11.2
  ```
go to /swagger to learn the api routes
