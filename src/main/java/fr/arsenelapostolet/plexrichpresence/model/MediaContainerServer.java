package fr.arsenelapostolet.plexrichpresence.model;

import lombok.Data;
import org.simpleframework.xml.Attribute;
import org.simpleframework.xml.ElementList;
import org.simpleframework.xml.Root;

import java.util.List;

@Data
@Root(name = "MediaContainer", strict = false)
public class MediaContainerServer {
    @Attribute
    private String friendlyName;

    @ElementList(inline = true, entry = "Server", type = Server.class)
    private List<Server> Server;
}
