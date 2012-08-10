Setting up the application.

1. Update Facebook application ID and application secret in DBCreator project. app.config file
   Look for <facebookSettings> section in configuration.
2. Update Facebook appId and app secret in kululu.web/web.config file too.
3. Create MySQL database and user with access to it on the development machine.
3. Fill in the details of the database, user and password in connection strings in DBCreator/app.config and kululu.web/Web.config.
4. kululu.web/Web.release.config: update the connection string, appId and appSecret.