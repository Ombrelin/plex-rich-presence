
package fr.arsenelapostolet.plexrichpresence.model;

import lombok.Data;

import java.util.List;

@Data
public class Part {

    public String container;
    public String duration;
    public String file;
    public String id;
    public String key;
    public String size;
    public List<Stream> stream = null;
}
