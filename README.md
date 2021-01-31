# Plex Rich Presence

Plex Rich Presence is a multiplatform Java GUI App that allows you to display your current PLEX session in your Discord Rich presence status.
Plex Rich Presence works with Java 8.

![icon](src/main/resources/images/icon.png | width=250 | height=250)

## Release Version

- [Portable and Executable JAR](https://github.com/Ombrelin/plex-rich-presence/releases/download/v1.3/plex-rich-presence-1.3.jar)
- [Windows Installer](https://github.com/Ombrelin/plex-rich-presence/releases/download/v1.3/plex-rich-presence-setup.exe)

## Screenshots

![screenshots](screenshots/ui-main.png)


![screenshots](screenshots/ui-logs.png)

## Build

Just run :

```
mvn clean package
```

## Run 

Once build, you can run the app by running :

```
java -jar ./target/plex-rich-presence-1.3.jar
```

## Libraries used

- JavaFX
- [RxJava](https://github.com/ReactiveX/RxJava)  
- [Spring Boot](https://github.com/spring-projects/spring-boot)
- [FxWeaver](https://github.com/rgielen/javafx-weaver)
- [JMetro](https://github.com/JFXtras/jfxtras-styles)
- [Java Discord RPC](https://github.com/MinnDevelopment/java-discord-rpc)
- [Retrofit 2](https://github.com/square/retrofit)
- [Project Lombok](https://github.com/rzwitserloot/lombok)

## Special Thanks

Thanks to [Discord](https://discord.com/) and [PLEX Media Server](https://plex.tv)
