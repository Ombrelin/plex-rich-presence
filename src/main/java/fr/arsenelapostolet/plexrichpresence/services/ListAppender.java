package fr.arsenelapostolet.plexrichpresence.services;

import ch.qos.logback.classic.spi.ILoggingEvent;
import ch.qos.logback.core.AppenderBase;
import fr.arsenelapostolet.plexrichpresence.controller.LogViewController;
import javafx.application.Platform;


public class ListAppender extends AppenderBase<ILoggingEvent> {

    @Override
    protected void append(ILoggingEvent LoggingEvent) {
        Platform.runLater(() -> LogViewController.logList.add(String.format("[%s] %s", LoggingEvent.getLevel(), LoggingEvent.getMessage())));
    }
}
