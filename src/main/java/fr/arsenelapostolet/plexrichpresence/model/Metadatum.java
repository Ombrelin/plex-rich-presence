
package fr.arsenelapostolet.plexrichpresence.model;

import lombok.Data;

import java.util.List;

@Data
public class Metadatum {

    public String addedAt;
    public String art;
    public String duration;
    public String grandparentArt;
    public String grandparentGuid;
    public String grandparentKey;
    public String grandparentRatingKey;
    public String grandparentThumb;
    public String grandparentTitle;
    public String guid;
    public String index;
    public String key;
    public String lastViewedAt;
    public String librarySectionID;
    public String librarySectionKey;
    public String librarySectionTitle;
    public String parentGuid;
    public String parentIndex;
    public String parentKey;
    public String parentRatingKey;
    public String parentThumb;
    public String parentTitle;
    public String ratingCount;
    public String ratingKey;
    public String sessionKey;
    public String summary;
    public String thumb;
    public String title;
    public String type;
    public String updatedAt;
    public String viewCount;
    public String viewOffset;
    public List<Medium> media = null;
    public User User;
    public Player Player;
}
