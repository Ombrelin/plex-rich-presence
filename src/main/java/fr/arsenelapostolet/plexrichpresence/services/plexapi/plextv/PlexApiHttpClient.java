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

    private static final String API_URL = "https://plex.tv";




    public PlexApiHttpClient() {



        BasicAuthInterceptor interceptor =
                new BasicAuthInterceptor(login, password);
        httpClientFactory.addInterceptor(interceptor);

        this.setHttp(new Retrofit.Builder()
                .client(httpClientFactory.build())
                .baseUrl(API_URL)
                .addConverterFactory(SimpleXmlConverterFactory.create())
                .addCallAdapterFactory(RxJavaCallAdapterFactory.create())
                .build());

    }
}
