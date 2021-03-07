package fr.arsenelapostolet.plexrichpresence.controller;

import javafx.application.Platform;
import javafx.stage.Stage;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import javax.imageio.ImageIO;
import java.awt.*;
import java.awt.event.MouseEvent;
import java.awt.event.MouseListener;
import java.io.IOException;

public class TrayIconController {

    private TrayIcon trayIcon;
    private final Logger LOG = LoggerFactory.getLogger(TrayIconController.class);
    private final Stage stage;

    private final MouseListener mouseListener = new MouseListener() {

        public void mouseClicked(MouseEvent e) {
            if (e.getClickCount() == 2 && e.getButton() == MouseEvent.BUTTON1) {
                Platform.runLater(() -> showStage());
            }
        }

        @Override
        public void mousePressed(MouseEvent e) {}

        @Override
        public void mouseReleased(MouseEvent e) {}

        @Override
        public void mouseEntered(MouseEvent e) {}

        @Override
        public void mouseExited(MouseEvent e) {}
    };

    public TrayIconController(Stage stage) {
        this.stage = stage;
    }

    // Returns true if system tray icon is initialized.
    public boolean initTrayIcon() {
        try {
            Toolkit.getDefaultToolkit();
            if (!SystemTray.isSupported()) {
                LOG.warn("No system tray support. Not initializing system tray icon.");
                return false;
            }

            SystemTray tray = SystemTray.getSystemTray();
            Image image = ImageIO.read(this.getClass().getClassLoader().getResource("images/icon.png"));
            trayIcon = new TrayIcon(image);

            trayIcon.setImageAutoSize(true);
            trayIcon.addMouseListener(this.mouseListener);

            java.awt.MenuItem exitItem = new java.awt.MenuItem("Exit");

            exitItem.addActionListener(event -> {
                Platform.exit();
                tray.remove(trayIcon);
                System.exit(0);
            });

            final PopupMenu popup = new PopupMenu();
            popup.add(exitItem);
            trayIcon.setPopupMenu(popup);
            tray.add(trayIcon);
            LOG.info("Tray icon initialized");
            return true;
        } catch (AWTException | IOException e) {
            LOG.error("Failed to initialize the system tray icon " + e.getMessage());
            return false;
        }
    }

    private void showStage() {
        if (stage != null) {
            stage.setIconified(false);
            stage.show();
            stage.toFront();
        }
    }

    public TrayIcon getTrayIcon() {
        return trayIcon;
    }
}
