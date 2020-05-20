using System;

public interface IAStarLinker {
    void getPath(PathRequest request, Action<PathResult> callback);
}