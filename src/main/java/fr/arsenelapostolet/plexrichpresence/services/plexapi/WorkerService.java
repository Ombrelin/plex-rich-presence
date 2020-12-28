package fr.arsenelapostolet.plexrichpresence.services.plexapi;

import javafx.concurrent.Service;
import javafx.concurrent.Task;

public class WorkerService extends Service {
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
                        e.printStackTrace();
                    }
                }

                return null;
            }
        };
    }
}
