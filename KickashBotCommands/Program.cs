﻿using System; using System.Collections.Generic; using System.Linq; using Discord; using Discord.Commands; using System.IO; using Newtonsoft.Json; using System.Text.RegularExpressions; using System.Threading.Tasks;  namespace KickashBotCommands { 	class Program 	{ 		static void Main(string[] args) 		{ 			new Program().start(); 		}  		private DiscordClient _client;  		static readonly string nl = Environment.NewLine; 		static readonly char pre = '/';  		Dictionary<string, string> emotes;  		static readonly string dirP = Directory.GetParent(Environment.CurrentDirectory).ToString(); 		static readonly string pathE = dirP + "/emotes.txt"; 		static readonly string pathL = dirP + "/login.txt"; 		static readonly string pathEv = dirP + "/events.txt";  		static string[] regions = { "North America", "South America", "Europe", "Africa", "Asia", "Oceania" };  		static readonly string ver = "1.5.1"; 		static readonly string app = "KrAB";  		HashSet<ulong> authorized; 		HashSet<ulong> channels;  		Dictionary<string, SignUp> events;  		public void start() 		{			 			_client = new DiscordClient(x => 			{ 				x.AppName = app; 				x.LogLevel = LogSeverity.Info; 				x.LogHandler = Log; 			});  			_client.UsingCommands(x => 			{ 				x.PrefixChar = pre; 				x.AllowMentionPrefix = true; 				x.HelpMode = HelpMode.Private; 				x.ErrorHandler = LogC; 			});  			try 			{ 				string json; 				using (StreamReader r = new StreamReader(pathEv)) 					json = r.ReadToEnd(); 				events = JsonConvert.DeserializeObject<Dictionary<string, SignUp>>(json); 			} 			catch 			{ 				events = new Dictionary<string, SignUp>(); 			}              authorized = new HashSet<ulong>(); 			emotes = new Dictionary<string, string>(); 			CreateCommands();             CreateWhitelist();              _client.UserJoined += _client_UserJoined; 			_client.Ready += _client_Ready; 			_client.MessageReceived += _client_MessageReceived; 			//_client.UsingCommands.  			_client.ExecuteAndWait(async () => 			{                 string tmp;                 using (StreamReader r = new StreamReader(pathL))                     tmp = r.ReadToEnd();                 string[] login = JsonConvert.DeserializeObject<string[]>(tmp);                  if (login[0] != "") 				    await _client.Connect(login[0], login[1]);                 else                     await _client.Connect(login[2]);             }); 		}

		private void CreateWhitelist() 		{ 			authorized.Add(191590594173206528); 			authorized.Add(191590784250675200);  			channels = new HashSet<ulong>(); 			channels.Add(191922218542956545); 			channels.Add(188435208045985792); 			channels.Add(195018339133816832); 			channels.Add(202265410765193216);  		}  		private void _client_UserJoined(object sender, UserEventArgs e) 		{ 			//e.User.SendMessage($"Welcome to {e.Server.Name}, {e.User.Mention}! I'm glad you've joined us!"); 		}  		private void _client_Ready(object sender, EventArgs e) 		{ 			updateStatus(); 			_client.GetChannel(195018339133816832).SendMessage("Back online!"); 		}  		private void updateStatus() 		{ 			_client.SetGame($"{pre}help {app} {ver}"); 		}  		public void CreateCommands() 		{ 			var cService = _client.GetService<CommandService>();  			/*cService.CreateCommand("hey")                 .Description("Welcomes you")                 .Do(async (e) =>                 {                     await e.Channel.SendMessage($"Hey {e.User.Mention}! How are you?");                     await e.Message.Delete();                 });*/  			cService.CreateCommand("add") 				.Description("adds an emote") 				.Parameter("cmd", ParameterType.Required) 				.Parameter("emote", ParameterType.Unparsed) 				.Do(async (e) => 				{ 					if (whitelist(e) && perm(e)) 					{ 						string msg = AddEmote(e.GetArg("cmd"), e.GetArg("emote")); 						saveEmotes();  						await e.Channel.SendMessage(msg); 					} 				});  			cService.CreateCommand("delete") 				.Description("Deletes selected or last emote command") 				.Parameter("cmd", ParameterType.Optional) 				.Do(async (e) => 				{ 					if (whitelist(e) && perm(e)) 					{ 						string remove = null; 						if (e.GetArg(0) == "") 							remove = emotes.Keys.Last(); 						else if (emotes.Keys.Contains(e.GetArg(0))) 							remove = e.GetArg(0); 						else 							await e.Channel.SendMessage("Emote not found");  						if (remove != null) 						{ 							emotes.Remove(remove); 							saveEmotes();  							await e.Channel.SendMessage($"deleteing {remove}");
							await Task.Delay(200);  							restart(); 						} 					} 				});  			cService.CreateCommand("version") 				.Description("Displays current version and updates 'game'") 				.Do(async (e) => 				{ 					await e.Channel.SendMessage(ver); 					updateStatus(); 				});  			cService.CreateCommand("reload") 				.Description("Reloads emote cache after manual addition") 				.Do(async (e) => 				{ 					if (whitelist(e) && perm(e)) 					{ 						AddEmotes(); 						await e.Channel.SendMessage("Reloaded"); 					} 				});  			cService.CreateCommand("changeRegion") 				.Description("Sets your region for identification: " + 				'`' + string.Join("`, `", regions) + '`') 				.Parameter("region", ParameterType.Unparsed) 				.Do(async (e) => 				{ 					if(channels.Contains(e.Channel.Id)) 					{
						var tmp = e.Server.Roles;
						string name;
						string input;
						foreach (var role in tmp)
						{
							name = Regex.Replace(role.Name.ToLower(), @"\s+", "");
							input = Regex.Replace(e.GetArg(0).ToLower(), @"\s+", "");
							if (name == input)
							{
								var tmp2 = e.User.Roles;
								foreach (var role2 in tmp2)
								{
									if (regions.Contains(role2.Name))
									{
										await e.User.RemoveRoles(role2);
									}
								}
								await e.User.AddRoles(role);
								await e.Channel.SendMessage($"Set {e.User.Name}'s region to {role.Name}");
							}
						} 					} 				});  			cService.CreateCommand("getID") 				.Description("Gets id from a mention") 					.Parameter("mention", ParameterType.Required) 				.Do(async (e) => 				{ 					if (whitelist(e)) 					{ 						await e.Channel.SendMessage(getMentionID(e.GetArg(0)).ToString()); 					} 				});  			cService.CreateCommand("say") 				.Description("Repeats after you") 					.Parameter("arg", ParameterType.Unparsed) 				.Do(async (e) => 				{ 					if (perm(e)) 					{ 						await e.Channel.SendMessage(e.GetArg(0)); 						await e.Message.Delete(); 					} 				});  			cService.CreateCommand("eventCreate") 		        .Description($"Creates a new sinup list fo events with a name and a time in format {nl} Thu, 01 May 2008 07:34:42 GMT") 				.Parameter("name", ParameterType.Required) 		        .Parameter("time", ParameterType.Unparsed) 				.Do(async (e) => 				{ 					if (whitelist(e) && perm(e)) 					{
						string name = e.GetArg(0).ToLower(); 						if (!events.ContainsKey(name)) 						{
							DateTime myTime;
							if (DateTime.TryParse(e.GetArg(1), out myTime))
							{
								var tmp = new SignUp(myTime);
								events.Add(name, tmp);
								await e.Message.Channel.SendMessage($"Event {name} Created");
								saveEvents();
							}
							else
							{ 								await e.Message.Channel.SendMessage($"Time format error");
							} 						} 						else 						{ 							await e.Message.Channel.SendMessage($"Event already created"); 						} 					} 				});  			cService.CreateCommand("eventDelete") 				.Description("Removes an event") 				.Parameter("event", ParameterType.Required) 				.Do(async (e) => 				{ 					if (whitelist(e) && perm(e)) 					{
						string name = e.GetArg(0).ToLower(); 						if (events.ContainsKey(name)) 						{ 							events.Remove(name); 							await e.Message.Channel.SendMessage($"Event {name} Deleted");  							saveEvents(); 						} 						else 						{ 							await e.Message.Channel.SendMessage($"Event not found"); 						} 					} 				});  			/*cService.CreateCommand("eventChangeInfo") 				.Description("Removes an event") 				.Parameter("event", ParameterType.Required) 				.Parameter("description", ParameterType.Unparsed) 				.Do(async (e) => 				{ 					if (whitelist(e) && perm(e)) 					{ 						if (events.ContainsKey(e.GetArg(0))) 						{ 							events[e.GetArg(0)].desc = e.GetArg(1); 							await e.Message.Channel.SendMessage($"Event {e.GetArg(0)} description modified");  							saveEvents(); 						} 						else 						{ 							await e.Message.Channel.SendMessage($"ERROR {e.GetArg(0)} not found"); 						} 					} 				});*/  			cService.CreateCommand("eventVoteTime")
				.Description($"Changes the event's voting deadline {nl} Format: {pre}eventvotetime eventname Thu, 01 May 2008 07:34:42 GMT") 				.Parameter("event", ParameterType.Required) 				.Parameter("date", ParameterType.Unparsed) 				.Do(async (e) => 				{ 					if (whitelist(e) && perm(e)) 					{
						string name = e.GetArg(0).ToLower(); 						if (events.ContainsKey(name)) 						{ 							var tmp = events[name];
							tmp.Deadline = DateTime.Parse(e.GetArg(1));
							await e.Message.Channel.SendMessage($"Event {name} voting deadline modified");

							saveEvents(); 						} 						else 						{ 							await e.Message.Channel.SendMessage("Event not found"); 						} 					} 				});  			cService.CreateCommand("eventVote")
				.Description($"Votes for in an event") 				.Parameter("event", ParameterType.Required) 				.Parameter("option", ParameterType.Unparsed) 				.Do(async (e) => 				{ 					if (whitelist(e) && perm(e)) 					{
						string name = e.GetArg(0).ToLower(); 						if (events.ContainsKey(name)) 						{
							if (events[name].vote(e.User.Id, e.GetArg(1)))
							{ 								await e.Message.Channel.SendMessage($"{e.User.Name} vote recorded");
								saveEvents(); 							}
							else
								await e.Message.Channel.SendMessage($"Option not found"); 						} 						else 						{
							await e.Message.Channel.SendMessage($"Event not found");
						}
					} 				});  			cService.CreateCommand("eventJoin") 				.Description("Signs you up for the event") 				.Parameter("event", ParameterType.Required) 				.Do(async (e) => 				{ 					if (whitelist(e)) 					{
						string name = e.GetArg(0).ToLower(); 						if (events.ContainsKey(name)) 						{ 							var tmp = events[name];
							tmp.attend(e.User.Id); 							await e.Channel.SendMessage($"{e.User.Name} will be attending: {name}");  							saveEvents(); 						} 						else 						{ 							await e.Channel.SendMessage("Event not found"); 						} 					} 				});  			cService.CreateCommand("eventLeave") 				.Description("Revokes your involvement from an event") 				.Parameter("event", ParameterType.Required) 				.Do(async (e) => 				{ 					if (whitelist(e)) 					{
						string name = e.GetArg(0).ToLower(); 						if (events.ContainsKey(name)) 						{ 							var tmp = events[name];
							tmp.unAttend(e.User.Id); 							await e.Channel.SendMessage($"{e.User.Name} is no longer attending: {name}"); 							saveEvents(); 						} 						else 						{ 							await e.Channel.SendMessage("Event not found"); 						} 					} 				});  			cService.CreateCommand("eventList") 				.Description("Lists all current events") 				.Do(async (e) => 				{ 					if (whitelist(e)) 					{ 						string msg = "Current events" + nl + "```"; 						foreach (var tmp in events.Keys) 						{ 							msg += tmp + nl; 						} 						await e.Channel.SendMessage(msg + "```"); 					} 				});  			cService.CreateCommand("eventInfo") 				.Description("Description of the event") 				.Parameter("event", ParameterType.Required) 				.Do(async (e) => 				{ 					if (whitelist(e)) 					{
						string name = e.GetArg(0).ToLower(); 						if (events.ContainsKey(name)) 						{ 							var tmp = events[name]; 							await e.Channel.SendMessage($"{name}{nl}`{tmp.ToString()}`"); 						} 						else 						{ 							await e.Channel.SendMessage("Event not found"); 						} 					} 				});  			cService.CreateCommand("eventUsers") 				.Description("Lists all people who have signed up for the event") 				.Parameter("event", ParameterType.Required) 				.Do(async (e) => 				{ 					if (whitelist(e)) 					{
						string name = e.GetArg(0).ToLower(); 						if (events.ContainsKey(name)) 						{ 							var tmp = events[name]; 							string msg = $"Users signed for {name}{nl}"; 							foreach (var id in tmp.users) 							{ 								var user = e.Server.GetUser(id); 								msg += $"{user.Name}{nl}"; 							} 							await e.Channel.SendMessage(msg); 						} 						else 						{ 							await e.Channel.SendMessage("Event not found"); 						} 					} 				});  			cService.CreateCommand("prune") 				.Description("Proxy for lapis purge") 		        .Parameter("args", ParameterType.Unparsed) 				.Do(async (e) => 				{ 					if (perm(e)) 					{ 						var msg = await e.Channel.SendMessage($"?prune {e.GetArg(0)}"); 						await Task.Delay(2000); 						await msg.Delete(); 					} 				});  			AddEmotes(); 		}  		private void AddEmotes() 		{ 			//var path = pathE; 			//var array = emotes;  			string json; 			using (StreamReader r = new StreamReader(pathE)) 				json = r.ReadToEnd(); 			var tmp = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);  			var keys = tmp.Keys; 			for (int i = 0; i < keys.Count; i++) 				AddEmote(keys.ElementAt(i), tmp[keys.ElementAt(i)]); 		}  		private string AddEmote(string cmd, string emote) 		{ 			var cService = _client.GetService<CommandService>();  			if (!emotes.ContainsKey(cmd)) 			{ 				emotes.Add(cmd, emote);  				cService.CreateCommand(cmd) 					.Description("emote") 					.Parameter("unused", ParameterType.Unparsed) 					.Do(async (e) => 					{ 						string msg = $"`{e.User.Name}` {nl}{emote}"; 						await e.Channel.SendMessage(msg);  						if (e.GetArg(0) == "") 							await e.Message.Delete(); 					}); 				return ($"{cmd} ADDED!"); 			}  			return ($"{cmd} was NOT added..."); 		}  		private async void _client_MessageReceived(object sender, MessageEventArgs e) 		{ 			if (e.Message.Text.Contains("┻━┻")) 			{ 				await e.Channel.SendMessage("┬─┬﻿ ノ( ゜-゜ノ)"); 			} 		}  		private static ulong getMentionID(string mention) 		{ 			string raw = Regex.Replace(mention, @"[^\d]", ""); 			return ulong.Parse(raw); 		}  		private bool perm(CommandEventArgs e) 		{ 			bool test = false; 			foreach(var role in e.User.Roles)             {                 test |= authorized.Contains(role.Id);             }  			if (test || e.User.Id == 140263256697602049) 			{ 				return true; 			} 			else 			{ 				e.Channel.SendMessage("UNAUTHORIZED"); 				return false; 			} 		}  		private bool whitelist(CommandEventArgs e) 		{ 			if (channels.Contains(e.Channel.Id)) 			{ 				return true; 			} 			else  			{ 				return false; 			} 		}  		private void saveEmotes() 		{ 			string json = JsonConvert.SerializeObject(emotes, Formatting.Indented); 			File.WriteAllText(pathE, json); 		} 		public void saveEvents() 		{ 			string json = JsonConvert.SerializeObject(events, Formatting.Indented); 			File.WriteAllText(pathEv, json); 		}  		private async void restart() 		{ 			await _client.Disconnect(); 			new Program().start(); 			//Environment.Exit(0); 		}  		public void Log(object sender, LogMessageEventArgs e) 		{ 			Console.WriteLine($"[{e.Severity}] [{e.Source}] {e.Message}"); 		}  		void LogC(object sender, CommandErrorEventArgs e)
		{
			Console.WriteLine($"[{e.Command}] [{e.ErrorType}] [{e.Exception}] {e.Message}");
		} 	} } 