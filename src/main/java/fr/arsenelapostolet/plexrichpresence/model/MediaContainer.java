package fr.arsenelapostolet.plexrichpresence.model;

import lombok.Data;

import java.util.List;

@Data
public class MediaContainer {

    public Integer size;
    public List<Metadatum> Metadata = null;
}
