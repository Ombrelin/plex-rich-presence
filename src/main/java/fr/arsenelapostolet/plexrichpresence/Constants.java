package fr.arsenelapostolet.plexrichpresence;

import com.sun.javafx.collections.ObservableListWrapper;
import javafx.collections.FXCollections;
import javafx.collections.ObservableList;

import java.util.ArrayList;

public final class Constants {
    public static final String plexProduct = "Discord_Plex_Rich_Presence";
    public static final String plexClientIdentifier = "nDwkFkJCCJQEjq44TDaLJwKW54";
    public static String authToken = "";
    public static ObservableList<String> logList = FXCollections.synchronizedObservableList(FXCollections.observableArrayList());
}
