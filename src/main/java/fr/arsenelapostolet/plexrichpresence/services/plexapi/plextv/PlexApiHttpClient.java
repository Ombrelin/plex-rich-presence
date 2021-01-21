package fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv;

import okhttp3.OkHttpClient;
import retrofit2.Retrofit;
import retrofit2.adapter.rxjava.RxJavaCallAdapterFactory;
import retrofit2.converter.simplexml.SimpleXmlConverterFactory;

public class PlexApiHttpClient extends PlexTvHttpClient{

    private static final String API_URL = "https://plex.tv";

    private final OkHttpClient.Builder httpClientFactory;

    public PlexApiHttpClient() {
        this.httpClientFactory = new OkHttpClient.Builder();


        //BasicAuthInterceptor interceptor =
        //        new BasicAuthInterceptor(login, password);
        //httpClientFactory.addInterceptor(interceptor);

        this.setHttp(new Retrofit.Builder()
                .client(this.httpClientFactory.build())
                .baseUrl(API_URL)
                .addConverterFactory(SimpleXmlConverterFactory.create())
                .addCallAdapterFactory(RxJavaCallAdapterFactory.create())
                .build());

    }
}
