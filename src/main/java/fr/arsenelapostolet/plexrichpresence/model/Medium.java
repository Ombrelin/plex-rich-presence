package fr.arsenelapostolet.plexrichpresence.model;

import lombok.Data;

import java.util.List;

@Data
public class Medium {

    public String audioChannels;
    public String audioCodec;
    public String bitrate;
    public String container;
    public String duration;
    public String id;
    public List<Part> part = null;
}
