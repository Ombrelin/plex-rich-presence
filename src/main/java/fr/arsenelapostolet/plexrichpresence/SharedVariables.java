package fr.arsenelapostolet.plexrichpresence;

import javafx.collections.FXCollections;
import javafx.collections.ObservableList;

public final class SharedVariables {
    public static final String plexProduct = "Discord_Plex_Rich_Presence";
    public static final String plexClientIdentifier = "nDwkFkJCCJQEjq44TDaLJwKW54";
    public static String authToken = "";
    public static ObservableList<String> logList = FXCollections.synchronizedObservableList(FXCollections.observableArrayList());
}
