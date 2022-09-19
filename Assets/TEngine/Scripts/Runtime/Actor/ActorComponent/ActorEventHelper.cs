namespace TEngine.Runtime.Actor
{
    public static class ActorEventHelper
    {
        public static void Send(GameActor actor, int eventId)
        {
            actor.Event.SendEvent(eventId);
        }

        public static void Send<T>(GameActor actor, int eventId, T info)
        {
            actor.Event.SendEvent<T>(eventId, info);
        }

        public static void Send<T, TU>(GameActor actor, int eventId, T info1, TU info2)
        {
            actor.Event.SendEvent<T, TU>(eventId, info1, info2);
        }

        public static void Send<T, TU, TV>(GameActor actor, int eventId, T info1, TU info2, TV info3)
        {
            actor.Event.SendEvent<T, TU, TV>(eventId, info1, info2, info3);
        }

        public static void Send<T, TU, TV, TW>(GameActor actor, int eventId, T info1, TU info2, TV info3, TW info4)
        {
            actor.Event.SendEvent<T, TU, TV, TW>(eventId, info1, info2, info3, info4);
        }
    }
}