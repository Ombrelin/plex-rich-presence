
package fr.arsenelapostolet.plexrichpresence.model;

import lombok.Data;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

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
