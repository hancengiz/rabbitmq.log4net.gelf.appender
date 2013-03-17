rabbitmq.log4net.gelf.appender
==============================

rabbitmq.log4net.gelf.appender

sample config
=============

<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler,Log4net" />
  </configSections>
  
  <log4net>
    <appender name="rabbitmq.gelf.appender" type="rabbitmq.log4net.gelf.appender.GelfRabbitMqAdapter, rabbitmq.log4net.gelf.appender">
      <HostName value="localhost" />
      <VirtualHost value="/" />
      <Port value="5672" />
      <Exchange value="log4net.gelf.appender" />
      <Username value="guest" />
      <Password value="guest" />
    </appender>

   
    <root>
      <level value="ERROR" />
      <appender-ref ref="rabbitmq.gelf.appender" />
    </root>
  </log4net>

</configuration>