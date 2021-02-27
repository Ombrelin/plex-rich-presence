package fr.arsenelapostolet.plexrichpresence;

import fr.arsenelapostolet.plexrichpresence.controller.MainController;
import javafx.application.Application;
import javafx.application.Platform;
import javafx.scene.Parent;
import javafx.scene.Scene;
import javafx.scene.image.Image;
import javafx.stage.Stage;
import jfxtras.styles.jmetro.JMetro;
import jfxtras.styles.jmetro.Style;
import net.rgielen.fxweaver.core.FxWeaver;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.boot.builder.SpringApplicationBuilder;
import org.springframework.context.ConfigurableApplicationContext;

import javax.imageio.ImageIO;
import javax.swing.*;
import java.awt.*;
import java.io.IOException;
import java.util.Properties;

public class PlexRefresherFX extends Application {

    private ConfigurableApplicationContext applicationContext;
    private final Logger LOG = LoggerFactory.getLogger(PlexRefresherFX.class);
    private Stage stage;
    private TrayIcon trayIcon;

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
        this.stage = stage;
        ConfigManager.initConfig();
        FxWeaver fxWeaver = applicationContext.getBean(FxWeaver.class);
        Parent root = fxWeaver.loadView(MainController.class);
        Scene scene = new Scene(root);
        JMetro jMetro = new JMetro(Style.DARK);
        jMetro.setScene(scene);
        scene.getStylesheets().add(getClass().getClassLoader().getResource("style.css").toExternalForm());
        scene.getStylesheets().add(getClass().getClassLoader().getResource("theme.css").toExternalForm());
        SwingUtilities.invokeLater(this::addAppToTray);
        this.stage.setMinWidth(350);
        this.stage.setMinHeight(150);
        stage.setResizable(false);
        this.stage.getIcons().add(new Image(getClass().getClassLoader().getResourceAsStream("images/icon.png")));
        this.stage.setTitle(String.format("Plex Rich Presence v%s", getVersion()));
        this.stage.setScene(scene);
        try {
            this.stage.show();
        } catch (Throwable e) {
            e.printStackTrace();
        }


        // On window close, minimise to tray.
        stage.setOnCloseRequest(event -> {
            hideStage();
        });

        // On window minimise, minimise to tray.
        stage.iconifiedProperty().addListener((ov, t, t1) -> {
            hideStage();
        });

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

    private void showStage() {
        if (stage != null) {
            stage.show();
            stage.toFront();
        }
    }

    private void hideStage() {
        stage.hide();
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

    private void addAppToTray() {
        try {
            java.awt.Toolkit.getDefaultToolkit();
            if (!java.awt.SystemTray.isSupported()) {
                System.out.println("No system tray support, application exiting.");
                Platform.exit();
            }

            java.awt.SystemTray tray = java.awt.SystemTray.getSystemTray();
            java.awt.Image image = ImageIO.read(this.getClass().getClassLoader().getResource("images/icon.png"));
            trayIcon = new java.awt.TrayIcon(image);

            trayIcon.setImageAutoSize(true);
            trayIcon.addActionListener(event -> Platform.runLater(this::showStage));

            java.awt.MenuItem exitItem = new java.awt.MenuItem("Exit");

            exitItem.addActionListener(event -> {
                Platform.exit();
                tray.remove(trayIcon);
                System.exit(0);
            });

            final java.awt.PopupMenu popup = new java.awt.PopupMenu();
            popup.add(exitItem);
            trayIcon.setPopupMenu(popup);
            tray.add(trayIcon);
        } catch (java.awt.AWTException | IOException e) {
            System.out.println("Unable to init system tray");
            e.printStackTrace();
        }
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
            trayIcon.displayMessage(title, message, TrayIcon.MessageType.INFO);
        }
    }

}
