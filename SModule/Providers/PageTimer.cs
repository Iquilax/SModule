using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace SModule.Providers
{
    public class PageTimer
    {
        private static Dictionary<String, Timer> facebookTimerDictionary = new Dictionary<string, Timer>();
        public static void stopTimer(String pageId)
        {
            Boolean isContain =  facebookTimerDictionary.ContainsKey(pageId);
            if (isContain)
            {
                Timer timer = facebookTimerDictionary[pageId];
                if (timer != null)
                {
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }            
        }
        public static void setTimer(Timer timer, string pageId)
        {
            Boolean isContain = facebookTimerDictionary.ContainsKey(pageId);
            if (!isContain)
            {
                facebookTimerDictionary.Add(pageId, timer);

            } else
            {
                stopTimer(pageId);
                facebookTimerDictionary[pageId] = timer;
            }
        }
        public static List<String> getAllCrawlingPage()
        {
            return facebookTimerDictionary.Keys.ToList();
        }
    }
}