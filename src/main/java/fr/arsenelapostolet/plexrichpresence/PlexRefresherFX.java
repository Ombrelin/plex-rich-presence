package fr.arsenelapostolet.plexrichpresence;

import fr.arsenelapostolet.plexrichpresence.controller.LogViewController;
import fr.arsenelapostolet.plexrichpresence.controller.MainController;
import fr.arsenelapostolet.plexrichpresence.controller.TrayIconController;
import javafx.application.Application;
import javafx.application.Platform;
import javafx.scene.Parent;
import javafx.scene.Scene;
import javafx.scene.image.Image;
import javafx.scene.layout.Pane;
import javafx.scene.layout.VBox;
import javafx.stage.Stage;
import jfxtras.styles.jmetro.JMetro;
import jfxtras.styles.jmetro.Style;
import net.rgielen.fxweaver.core.FxControllerAndView;
import net.rgielen.fxweaver.core.FxWeaver;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.boot.builder.SpringApplicationBuilder;
import org.springframework.context.ConfigurableApplicationContext;

import javax.swing.*;
import java.awt.*;
import java.io.IOException;
import java.util.Properties;

public class PlexRefresherFX extends Application {

    private ConfigurableApplicationContext applicationContext;
    private final Logger LOG = LoggerFactory.getLogger(PlexRefresherFX.class);
    private Stage logViewStage;
    private Stage mainWindowStage;
    private TrayIconController trayIconController;
    FxWeaver fxWeaver;


    @Override
    public void init() {
        String[] args = getParameters().getRaw().toArray(new String[0]);
        Platform.setImplicitExit(false);
        this.applicationContext = new SpringApplicationBuilder()
                .sources(PlexRichPresenceApplication.class)
                .headless(false)
                .run(args);
    }

    @Override
    public void start(Stage stage) {
        try {
            fxWeaver = applicationContext.getBean(FxWeaver.class);
            this.mainWindowStage = stage;
            trayIconController = new TrayIconController(stage);
            ConfigManager.initConfig();

            //Init main window
            ControllerAndStage<MainController> mainWindow = initWindow(mainWindowStage, MainController.class, VBox.class, String.format("Plex Rich Presence v%s", getVersion()), false);

            //Init log window
            ControllerAndStage<LogViewController> logWindow = initWindow(new Stage(), LogViewController.class, VBox.class, "Logs", true);
            logViewStage = logWindow.stage;

            // Only set on close/minimise event handlers if tray icon was initialized successfully.
            SwingUtilities.invokeLater(() -> {
                if (trayIconController.initTrayIcon()) {
                    // On window close, minimise to tray.
                    mainWindowStage.setOnCloseRequest(event -> {
                        hideStage();
                    });

                    // On window minimise, minimise to tray.
                    mainWindowStage.iconifiedProperty().addListener((ov, t, t1) -> {
                        hideStage();
                    });
                }
            });

            mainWindow.controller.setLogViewStage(logViewStage);

            this.mainWindowStage.show();
        } catch (Throwable e) {
            // This try catch block is here so that it can print the stacktrace + error message in console. It does not print the error nor stack trace without it for some reason)
            e.printStackTrace();
            Platform.exit();
            System.exit(1);
        }

    }

    private ControllerAndStage initWindow(Stage newStage, Class<?> controllerClass, Class<? extends Pane> parentType, String windowTitle, boolean isResizable) {
        FxControllerAndView controllerAndView = fxWeaver.load(controllerClass);
        Parent controllerParent = parentType.cast(controllerAndView.getView().orElseThrow());
        Scene controllerScene = new Scene(controllerParent);
        JMetro jMetro = new JMetro(Style.DARK);
        jMetro.setScene(controllerScene);
        controllerScene.getStylesheets().add(getClass().getClassLoader().getResource("style.css").toExternalForm());
        controllerScene.getStylesheets().add(getClass().getClassLoader().getResource("theme.css").toExternalForm());

        newStage.getIcons().add(new Image(getClass().getClassLoader().getResourceAsStream("images/icon.png")));
        newStage.setTitle(windowTitle);
        newStage.setResizable(isResizable);
        newStage.setScene(controllerScene);

        return new ControllerAndStage() {{
            controller = controllerAndView.getController();
            stage = newStage;
        }};
    }

    private static class ControllerAndStage<T> {
        T controller;
        Stage stage;
    }

    private String getVersion() {
        final Properties properties = new Properties();
        try {
            properties.load(getClass().getClassLoader().getResourceAsStream("application.properties"));
        } catch (IOException e) {
            e.printStackTrace();
        }
        return properties.getProperty("version");
    }

    private void hideStage() {
        mainWindowStage.setIconified(false);
        mainWindowStage.hide();
        new Thread(() -> {
            try {
                showSystemNotification("Plex Discord Rich Presence", "The application was minimized to tray. Open or exit the application via the tray icon.");
            } catch (IOException e) {
                LOG.error("Error showing system notification " + e.getMessage());
            }
        }).start();
    }

    @Override
    public void stop() {
        ConfigManager.saveConfig();
        this.applicationContext.close();
        Platform.exit();
        System.exit(0);
    }

    private void showSystemNotification(String title, String message) throws IOException {
        String os = System.getProperty("os.name");
        if (os.contains("Linux")) {
            ProcessBuilder builder = new ProcessBuilder(
                    "zenity",
                    "--notification",
                    "--title=" + title,
                    "--text=" + message);
            builder.inheritIO().start();
        } else if (os.contains("Mac")) {
            ProcessBuilder builder = new ProcessBuilder(
                    "osascript", "-e",
                    "display notification \"" + message + "\""
                            + " with title \"" + title + "\"");
            builder.inheritIO().start();
        } else if (SystemTray.isSupported()) {
            trayIconController.getTrayIcon().displayMessage(title, message, TrayIcon.MessageType.INFO);
        }
    }

}
