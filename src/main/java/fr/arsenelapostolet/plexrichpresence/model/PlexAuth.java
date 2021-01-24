package fr.arsenelapostolet.plexrichpresence.model;

import lombok.Data;

@Data
public class PlexAuth {
    public int id;
    public String code;
    public String authToken;
}
