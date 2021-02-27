package fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv;

import retrofit2.Retrofit;
import retrofit2.adapter.rxjava.RxJavaCallAdapterFactory;
import retrofit2.converter.simplexml.SimpleXmlConverterFactory;

public class PlexApiHttpClient extends PlexTvHttpClient {

    private static final String API_URL = "https://plex.tv";


    public PlexApiHttpClient() {


        this.setHttp(new Retrofit.Builder()
                .client(httpClientFactory.build())
                .baseUrl(API_URL)
                .addConverterFactory(SimpleXmlConverterFactory.create())
                .addCallAdapterFactory(RxJavaCallAdapterFactory.create())
                .build());

    }
}
