package fr.arsenelapostolet.plexrichpresence.services.plexapi.plextv;

import fr.arsenelapostolet.plexrichpresence.Constants;
import fr.arsenelapostolet.plexrichpresence.services.plexapi.PlexApiImpl;
import okhttp3.OkHttpClient;
import okhttp3.logging.HttpLoggingInterceptor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import retrofit2.Retrofit;
import retrofit2.adapter.rxjava.RxJavaCallAdapterFactory;
import retrofit2.converter.simplexml.SimpleXmlConverterFactory;

public class PlexApiHttpClient extends PlexTvHttpClient{
    private final Logger LOG = LoggerFactory.getLogger(PlexApiImpl.class);
    private static final String API_URL = "https://plex.tv";

    private final OkHttpClient.Builder httpClientFactory;
    HttpLoggingInterceptor logging = new HttpLoggingInterceptor( new HttpLoggingInterceptor.Logger()
    {
        @Override public void log(String message)
        {

            LOG.debug(message.replace(Constants.authToken, "**REDACTED**"));
        }
    });

    public PlexApiHttpClient() {
        logging.setLevel(HttpLoggingInterceptor.Level.BODY);
        this.httpClientFactory = new OkHttpClient.Builder().addInterceptor(logging);

        this.setHttp(new Retrofit.Builder()
                .client(this.httpClientFactory.build())
                .baseUrl(API_URL)
                .addConverterFactory(SimpleXmlConverterFactory.create())
                .addCallAdapterFactory(RxJavaCallAdapterFactory.create())
                .build());

    }
}
