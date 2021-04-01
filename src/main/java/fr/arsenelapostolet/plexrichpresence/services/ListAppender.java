package fr.arsenelapostolet.plexrichpresence.services;

import ch.qos.logback.classic.spi.ILoggingEvent;
import ch.qos.logback.core.AppenderBase;
import javafx.application.Platform;

import static fr.arsenelapostolet.plexrichpresence.SharedVariables.logList;

public class ListAppender extends AppenderBase<ILoggingEvent> {

    @Override
    protected void append(ILoggingEvent LoggingEvent) {
        Platform.runLater(() -> logList.add(String.format("[%s] %s", LoggingEvent.getLevel(), LoggingEvent.getMessage())));
    }
}
