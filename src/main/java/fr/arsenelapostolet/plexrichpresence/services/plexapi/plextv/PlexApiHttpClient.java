package fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv;

import okhttp3.OkHttpClient;
import retrofit2.Retrofit;
import retrofit2.converter.simplexml.SimpleXmlConverterFactory;

public class PlexApiHttpClient extends PlexTvHttpClient{

    private static String API_URL = "https://plex.tv";

    private OkHttpClient.Builder httpClientFactory;


    private String login;
    private String password;

    public PlexApiHttpClient(String login, String password) {
        this.httpClientFactory = new OkHttpClient.Builder();
        this.login = login;
        this.password = password;

        BasicAuthInterceptor interceptor =
                new BasicAuthInterceptor(login, password);
        httpClientFactory.addInterceptor(interceptor);

        this.setHttp(new Retrofit.Builder()
                .client(this.httpClientFactory.build())
                .baseUrl(API_URL)
                .addConverterFactory(SimpleXmlConverterFactory.create())
                .build());

    }
}
