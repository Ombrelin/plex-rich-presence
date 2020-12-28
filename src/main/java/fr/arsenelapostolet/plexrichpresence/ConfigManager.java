package fr.arsenelapostolet.plexrichpresence;

import java.io.*;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Properties;

public class ConfigManager {
    private static Properties config;
    private static File configFile;

    public static void initConfig() {
        Path userHomeDir = Paths.get(System.getProperty("user.home"));
        Path configDir = Paths.get(userHomeDir.toString(), ".plexdiscordrp");
        configFile = new File(configDir.toString() + "/config.properties");

        if (!Files.isDirectory(configDir)) {
            try {
                Files.createDirectory(configDir);
            } catch (IOException e) {
                System.out.println("Unable to create config directory. \n" + e.getMessage());
            }
        }

        if (!configFile.exists()) {
            try {
                configFile.createNewFile();
            } catch (IOException e) {
                System.out.println("Unable to create config file. \n" + e.getMessage());
            }

        }

        try (InputStream input = new FileInputStream(configFile)) {

            config = new Properties();
            config.load(input);

        } catch (IOException e) {
            System.out.println("Unable to open config file. \n" + e.getMessage());
        }
    }

    public static String getConfig(String configKey) {
        return config.getProperty(configKey);
    }

    public static void setConfig(String configKey, String configValue) {
        config.setProperty(configKey, configValue);
    }

    public static void saveConfig() {
        try (OutputStream output = new FileOutputStream(configFile)) {

            config.store(output, null);

        } catch (IOException e) {
            System.out.println("Unable to save config file. \n" + e.getMessage());
        }
    }
}
