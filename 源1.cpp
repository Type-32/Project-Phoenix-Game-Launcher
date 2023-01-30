#include <iostream>
#include <algorithm>
#include <cstring>
#include <string>
#include <string.h>
#include <map>
#include <vector>
#define maxNum 1000010
using namespace std;
int N, tempPath[maxNum], start[maxNum], end[maxNum];
map<float, int> pathCount, sub;
vector<string> ans;
string dfs(int currentStep, map<float, int> steps, string route, int previousMax)
{
    if (steps == pathCount)
    {
        int cnt = 0;
        for (int i = 0; i < strlen(route) - 1; i++)
        {
            if (route[i] != route[i + 1])
            {
                cnt++;
            }
        }
        if (previousMax < cnt)
        {
            previousMax = cnt;
            ans.push_front(route);
        }
    }
    if ((currentStep <= N) && (steps[(float)currentStep + 0.5f] > pathCount[(float)currentStep + 0.5f]))
        return route;
    if ((currentStep >= 0) && (steps[(float)currentStep - 0.5f] > pathCount[(float)currentStep - 0.5f]))
        return route;
    int backStep = currentStep - 1, frontStep = currentStep + 1;
    if (backStep < 0)
    {
        steps[(float)currentStep + 0.5f]++;
        dfs(frontStep, steps, route + "R", previousMax);
    }
    else if (frontStep > N)
    {
        steps[(float)currentStep - 0.5f]++;
        dfs(backStep, steps, route + "L", previousMax);
    }
    else
    {
        steps[(float)currentStep + 0.5f]++;
        dfs(frontStep, steps, route + "R", previousMax);
        steps[(float)currentStep + 0.5f]--;
        steps[(float)currentStep - 0.5f]++;
        dfs(backStep, steps, route + "L", previousMax);
        steps[(float)currentStep - 0.5f]--;
    }
    return route;
}
int main()
{
    cin >> N;
    for (int i = 0; i < N; i++)
    {
        cin >> tempPath[i];
        if (i < N)
            pathCount[i + 0.5f] = tempPath[i];
    }
    string strStep;
    dfs(0, sub, strStep, INT_MAX);
    cout << ans[0] << endl;
    return 0;
}