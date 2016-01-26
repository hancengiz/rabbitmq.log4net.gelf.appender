rabbitmq.log4net.gelf.appender
==============================
![Logo](/logo.png "Logo")

The RabbitMQ GELF appender is a customer appender for the [log4net](http://logging.apache.org/log4net/) logging framework which publishes log messages onto [RabbitMQ](http://www.rabbitmq.com/) message bus using the GELF (json) format.

## USAGE

### download the nuget package from the repository
https://nuget.org/packages/rabbitmq.log4net.gelf.appender/

### change your app/web config file

sample config
```  
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler,Log4net" />
  </configSections>
  
  <log4net>
    <appender name="rabbitmq.gelf.appender" type="rabbitmq.log4net.gelf.appender.GelfRabbitMqAppender, rabbitmq.log4net.gelf.appender">
      <HostName value="localhost" />
      <VirtualHost value="/" />
      <Port value="5672" />
      <Exchange value="log4net.gelf.appender" />
      <Username value="guest" />
      <Password value="guest" />
      <Facility value="sample-application" />
    </appender>

   
    <root>
      <level value="ERROR" />
      <appender-ref ref="rabbitmq.gelf.appender" />
    </root>
  </log4net>

</configuration>
```  

### Gelf Format 
https://github.com/Graylog2/graylog2-docs/wiki/GELF

## Use case

Sending your log messages onto a message bus (RabbitMQ) means they can be picked up easily and processed by a variety of consumers.

In particular it's handy with a log aggregator like [LogStash](http://logstash.net/). Just configure the [RabbitMQ input](http://logstash.net/docs/1.2.2/inputs/rabbitmq) and use [Kibana](http://www.elasticsearch.org/overview/kibana/) to search the [ElasticSearch](http://www.elasticsearch.org/overview/) database of your logs.

## CHANGE LOG

### Version 0.2

 * If logger logs an `Exception` tge GELF message is populated with additional information: `_ExceptionType`, `_ExceptionStackTrace`, `_InnerExceptionType`, `_InnerExceptionMessage`
 * The same happens when an exception object is provided when logging a message
 * More information from the LoggingEvent is populated in the GELF message: `_ThreadName` and `_Domain`
 * The `full_message` is only populated from a string when the original string is too long for `short_message`
 * When the Message Formatters don't populate `short_message` it's populated with the type of the logged object

### Version 0.1.7

 * A property called `message` (case insensitive) in a logged object is mapped to GELF `short_message`

### Version 0.1.6

 * Explicit Client Properties for RabbitMQ connections make it easier to identify connected appenders

### Version 0.1.5

 * You can specify `Facility` for the GELF message in the appender config file
