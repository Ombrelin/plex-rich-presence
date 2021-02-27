package fr.arsenelapostolet.plexrichpresence.services.plexapi;

import fr.arsenelapostolet.plexrichpresence.viewmodel.MainViewModel;
import javafx.concurrent.Service;
import javafx.concurrent.Task;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class WorkerService extends Service {
    private final Logger LOG = LoggerFactory.getLogger(MainViewModel.class);
    @Override
    protected Task<Void> createTask() {
        return new Task<Void>() {
            @Override
            protected Void call() throws Exception {

                for(int i=0; i<100; i++){
                    updateProgress(i, 100);
                    try {
                        Thread.sleep(100);
                    } catch (InterruptedException e) {
                        LOG.debug("WorkerService stopped.");
                    }
                }

                return null;
            }
        };
    }
}