package fr.arsenelapostolet.plexrichpresence.model;

import lombok.Data;

@Data
public class Player {

    public String address;
    public String device;
    public String machineIdentifier;
    public String model;
    public String platform;
    public String platformVersion;
    public String product;
    public String profile;
    public String remotePublicAddress;
    public String state;
    public String title;
    public String vendor;
    public String version;
    public Boolean local;
    public Boolean relayed;
    public Boolean secure;
    public Integer userID;

}
