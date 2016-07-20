using System;
using System.Collections.Generic;
using System.Timers;

namespace KickashBotCommands
{
	public class SignUp
	{
		int voteLimit;
		public int VoteLimit
		{
			get { return voteLimit; }
			set
			{
				voteLimit = value;
			}
		}
        Dictionary<string, HashSet<ulong>> votes;
        public HashSet<ulong> users { get; private set;}

		DateTime deadline;
        public DateTime Deadline {
            get { return deadline; }
            set
            {
                deadline = value;

				TimeSpan span = value - DateTime.Now;
				ulong ms = (ulong)span.TotalMilliseconds;

                aTimer = new Timer();
                aTimer.Elapsed += voteEnd;
                aTimer.Interval = ms;
                aTimer.Enabled = true;
            }
        }

		DateTime time;
        string desc;

        Timer aTimer;

        public SignUp(DateTime Time)
		{
			time = Time;

			users = new HashSet<ulong>();
            votes = new Dictionary<string, HashSet<ulong>>();

			VoteLimit = 1;

			//desc = $"Game: TBD {Environment.NewLine} Date: {time.DayOfWeek}, {time.Month} {time.Day} {Environment.NewLine} Time: {time.Hour} EST {Environment.NewLine}";
        }

        public void addOption(string option)
        {
            votes.Add(option, new HashSet<ulong>());
        }

		public void attend(ulong id)
		{
			users.Add(id);
		}


		public void unAttend(ulong id)
		{
			users.Remove(id);
		}

        public bool vote(ulong id, string option)
        {
            if (aTimer.Enabled)
            {
				int count = 0;
				foreach (var hash in votes.Values)
				{
					if (hash.Contains(id))
						count++;
				}
				if (count < VoteLimit)
				{
					votes[option].Add(id);
					return true;
				}
            }
			return true;
        }

        private void voteEnd(object sender, ElapsedEventArgs e)
        {
            aTimer.Enabled = false;
        }

        public override string ToString()
        {
			if (aTimer == null)
			{
				return $"Game: TBD {Environment.NewLine} Date: {time.DayOfWeek}, {time.Month} {time.Day} {Environment.NewLine} Time: {time.Hour} EST";
			}
			else if (aTimer.Enabled)
			{
				return $"Game: TBD {Environment.NewLine} Date: {time.DayOfWeek}, {time.Month} {time.Day} {Environment.NewLine} Time: {time.Hour} EST {Environment.NewLine} Voting Deadline: {time.DayOfWeek}, {time.Month} {time.Day}, {time.Hour} EST";
			}
			else
			{
				int count = 0;
				string key = "";
                foreach (var game in votes.Keys)
                {
                    if (votes[game].Count > count)
                    {
                        count = votes[game].Count;
                        key = game;
                    }
                }
                return $"Game: {key} {Environment.NewLine} Date: {time.DayOfWeek}, {time.Month} {time.Day} {Environment.NewLine} Time: {time.Hour} EST";
            }
        }
    }
}

