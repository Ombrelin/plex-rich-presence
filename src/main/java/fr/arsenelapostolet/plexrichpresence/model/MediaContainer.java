package fr.arsenelapostolet.plexrichpresence.model;

import lombok.Data;
import org.simpleframework.xml.Attribute;
import org.simpleframework.xml.Element;
import org.simpleframework.xml.Path;
import org.simpleframework.xml.Root;

@Data
@Root(name ="MediaContainer", strict = false)
public class MediaContainer {
    @Attribute
    private String friendlyName;

    @Element(name = "Server", required = false, type = Server.class)
    private Server Server;
}
