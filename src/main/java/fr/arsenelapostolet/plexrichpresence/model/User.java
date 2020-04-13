package fr.arsenelapostolet.plexrichpresence.model;

import lombok.Data;

@Data
public class User {

    private Integer id;
    private String uuid;
    private String email;
    private String joinedAt;
    private String username;
    private String title;
    private String thumb;
    private Boolean hasPassword;
    private String authToken;
    private String authenticationToken;
}