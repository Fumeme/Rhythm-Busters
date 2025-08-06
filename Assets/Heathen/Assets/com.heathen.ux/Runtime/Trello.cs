#if HE_SYSCORE

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HeathenEngineering.UX.API
{
    public static class Trello
    {
		public class ActionStatus
        {
			public bool hasError;
			[Obsolete("Use ActionStatus.result instead")]
			public bool isNetworkError => result == UnityWebRequest.Result.ConnectionError;
			[Obsolete("Use ActionStatus.result instead")]
			public bool isHttpError => result == UnityWebRequest.Result.DataProcessingError || result == UnityWebRequest.Result.ProtocolError;
			public UnityWebRequest.Result result;
			public long responceCode;
			public string errorMessage;

			public ActionStatus()
            {

            }

			public ActionStatus(string errorMessage)
			{
				hasError = true;
				this.errorMessage = errorMessage;
			}

			public ActionStatus(UnityWebRequest webRequest)
            {
				hasError = true;
				result = webRequest.result;
				responceCode = webRequest.responseCode;
				errorMessage = webRequest.error;
			}
        }

		[Serializable]
		private class BoardQueryResponce
        {
			public List<Board> boards = new List<Board>();
		}

		[Serializable]
		public class Board
		{
			public string name = "";
			public bool closed = false;
			public string idOrganization = "";
			public string id = "";
		}

		[Serializable]
		private class ListQueryResponce
		{
			public List<List> lists = new List<List>();
		}

		[Serializable]
		public class List
        {
			public string name = "";
			public string idBoard = "";
			public string pos = "bottom";
			public string id = "";
		}

		[Serializable]
		private class CardQueryResponce
        {
			public List<Card> cards;
		}

		[Serializable]
		public class Card
        {
			public string name = "";
			public string desc = "";
			public string descData = "";
			public string email = "";
			public string pos = "top";
			public string due = "null";
			public string idList = "";
			public string urlSource = "null";
		}

		[Serializable]
		private class CardCreateResponce
        {
			public string id;
        }

        public static class Url
        {
            public static string members = "https://api.trello.com/1/members/me";
            public static string boards = "https://api.trello.com/1/boards/";
            public static string cards = "https://api.trello.com/1/cards/";
            public static string lists = "https://api.trello.com/1/lists/";
        }

		[Serializable]
		public class ListContext
        {
			public string context;
			public string id;
        }

		/// <summary>
		/// Opens the default browser to the Trello App Key generation page.
		/// </summary>
		public static void ShowAppKeyPage()
        {
			Application.OpenURL("https://trello.com/app-key/");
        }

		/// <summary>
		/// Opens the default browser to the Trello Api Token generation page.
		/// </summary>
		/// <param name="apiKey">The App Api Key of the App the generated token will be for</param>
		/// <param name="tokenName">The name of the app to appear on the key</param>
		/// <param name="experation">When the token should expire. Examples<code>1hour</code><code>1day</code><code>30days</code><code>never</code>Never is the default</param>
		/// <param name="read">Should the read permision be enabled</param>
		/// <param name="write">Should the write permision be enabled</param>
		/// <param name="account"></param>
		public static void ShowApiTokenPage(string apiKey, string tokenName, string experation = "never", bool read = true, bool write = true, bool account = true)
        {
			if(!string.IsNullOrEmpty(apiKey))
            {
				string scope = "";
				if (read)
					scope += "read";
				if(write)
                {
					if (!string.IsNullOrEmpty(scope))
						scope += ",write";
					else
						scope += "write";
                }
				if(account)
                {
					if (!string.IsNullOrEmpty(scope))
						scope += ",account";
					else
						scope += "account";
                }

				if (string.IsNullOrEmpty(scope))
					return;

				if (string.IsNullOrEmpty(experation))
					return;

				Application.OpenURL("https://trello.com/1/authorize?expiration=" + experation + "&name=" + tokenName + "&scope=" + scope + "&response_type=token&key=" + apiKey);
				
            }
			else
            {
				Debug.LogWarning("The provided Api Key must not be blank!");
            }
        }

		/// <summary>
		/// Get the list of boards for the user indicated by the token.
		/// </summary>
		/// <param name="callback"><code>{ List[Board] results, ActionStatus status }</code>This is called when the process completes.</param>
		/// <returns></returns>
		public static IEnumerator GetBoards(string apiKey, string token, Action<List<Board>, ActionStatus> callback)
		{
			if (callback != null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(token))
			{
				string response = "";
				string request = Url.members + "?" + "key=" + apiKey + "&token=" + token + "&boards=all";
				UnityWebRequest www = UnityWebRequest.Get(request);

				var ao = www.SendWebRequest();

				while (!ao.isDone)
				{
					yield return null;
				}

				if (www.result != UnityWebRequest.Result.Success)
				{
					
					Debug.LogError("Trello API: [" + www.responseCode + "] " + www.downloadHandler.text);
					callback(null, new ActionStatus(www));
				}
				else
				{
					response = www.downloadHandler.text;
				}

				if (!string.IsNullOrEmpty(response))
				{
					try
					{
						var result = JsonUtility.FromJson<BoardQueryResponce>(response);
						callback(result.boards, new ActionStatus());
					}
					catch (Exception ex)
					{
						Debug.LogError("Trello API: [JSON failed to parse] " + ex.Message);
						callback(null, new ActionStatus(www));
					}
				}
			}
			else
			{
				if (callback != null)
				{
					var status = new ActionStatus("Trello API: Attempted to call the Trello API without first intializing the system.");
					Debug.LogError(status.errorMessage);
					callback.Invoke(null, status);
				}
				yield return null;
			}
		}

		/// <summary>
		/// Get the list of lists assoceated with the target board
		/// </summary>
		/// <param name="boardId">The board to query</param>
		/// <param name="callback"><code>{ List[List] results, ActionStatus status }</code>This is called when the process completes.</param>
		/// <returns></returns>
		public static IEnumerator GetLists(string apiKey, string token, string boardId, Action<List<List>, ActionStatus> callback)
        {
			if (callback != null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(token))
			{
				if (boardId == "")
				{
					var status = new ActionStatus("Trello API: Attempted to call GetLists without providing a board id.");
					Debug.LogError(status.errorMessage);
					callback.Invoke(null, status);
				}
				else
				{

					string response = "";
					string request = Url.boards + boardId + "?" + "key=" + apiKey + "&token=" + token + "&lists=all";
					UnityWebRequest www = UnityWebRequest.Get(request);

					var ao = www.SendWebRequest();

					while (!ao.isDone)
					{
						yield return null;
					}

					if (www.result != UnityWebRequest.Result.Success)
					{
						Debug.LogError("Trello API: [" + www.responseCode + "] " + www.downloadHandler.text);
						callback(null, new ActionStatus(www));
					}
					else
					{
						response = www.downloadHandler.text;
					}

					if (!string.IsNullOrEmpty(response))
					{
						try
						{
							var result = JsonUtility.FromJson<ListQueryResponce>(response);
							callback(result.lists, new ActionStatus());
						}
						catch (Exception ex)
						{
							Debug.LogError("Trello API: [JSON failed to parse] " + ex.Message);
							callback(null, new ActionStatus(www));
						}
					}
				}
			}
			else
            {
				if (callback != null)
				{
					var status = new ActionStatus("Trello API: Attempted to call the Trello API without first intializing the system.");
					Debug.LogError(status.errorMessage);
					callback.Invoke(null, status);
				}
				yield return null;
			}
		}

		/// <summary>
		/// Get the list of cards assoceated with the target list
		/// </summary>
		/// <param name="listId">The list to query</param>
		/// <param name="callback"><code>{ List[Card] results, ActionStatus status }</code>This is called when the process completes.</param>
		/// <returns></returns>
		public static IEnumerator GetCards(string apiKey, string token, string listId, Action<List<Card>, ActionStatus> callback)
		{
			if (callback != null || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(token))
			{
				if (listId == "")
				{
					var status = new ActionStatus("Trello API: Attempted to call GetCards without providing a list id.");
					Debug.LogError(status.errorMessage);
					callback.Invoke(null, status);
				}
				else
				{
					string response = "";
					string requestURL = Url.lists + listId + "?" + "key=" + apiKey + "&token=" + token + "&cards=all";
					UnityWebRequest www = UnityWebRequest.Get(requestURL);

					var ao = www.SendWebRequest();

					while (!ao.isDone)
					{
						yield return null;
					}

					if (www.result != UnityWebRequest.Result.Success)
					{
						Debug.LogError("Trello API: [" + www.responseCode + "] " + www.downloadHandler.text);
						callback(null, new ActionStatus(www));
					}
					else
					{
						response = www.downloadHandler.text;
					}

					if (!string.IsNullOrEmpty(response))
					{
						try
						{
							var results = JsonUtility.FromJson<CardQueryResponce>(response);
							callback(results.cards, new ActionStatus());
						}
						catch (Exception ex)
						{
							Debug.LogError("Trello API: [JSON failed to parse] " + ex.Message);
							callback(null, new ActionStatus(www));
						}
					}
				}
			}
			else
			{
				if (callback != null)
				{
					var status = new ActionStatus("Trello API: Attempted to call the Trello API without first intializing the system.");
					Debug.LogError(status.errorMessage);
					callback.Invoke(null, status);
				}
				yield return null;
			}
		}

		/// <summary>
		/// Creates a card on the target list and attaches the referenced files
		/// </summary>
		/// <param name="name">The name of the new card</param>
		/// <param name="description">The description of the card</param>
		/// <param name="listId">The ID of the list to attach it to</param>
		/// <param name="attachments">The attachments to upload</param>
		/// <param name="callback">Called when the process is complete</param>
		/// <returns></returns>
		public static IEnumerator CreateCard(string apiKey, string token, string listId, string name, string description, IEnumerable<string> attachments, Action<ActionStatus> callback)
        {
			if(string.IsNullOrEmpty(name))
            {
				var status = new ActionStatus("Trello API: Attempted to call CreateCard without providing a card name.");
				Debug.LogError(status.errorMessage);
				callback?.Invoke(status);
			}
			else if(string.IsNullOrEmpty(listId))
            {
				var status = new ActionStatus("Trello API: Attempted to call CreateCard without providing a list id.");
				Debug.LogError(status.errorMessage);
				callback?.Invoke(status);
			}
			else
            {
				WWWForm form = new WWWForm();
				form.AddField("key", apiKey);
				form.AddField("token", token);
				form.AddField("name", name);
				form.AddField("desc", description);
				form.AddField("idList", listId);

				UnityWebRequest www = UnityWebRequest.Post(Url.cards, form);

				var ao = www.SendWebRequest();

				while (!ao.isDone)
				{
					yield return null;
				}

				if (www.result != UnityWebRequest.Result.Success)
				{
					Debug.LogError("Trello API: [" + www.responseCode + "] " + www.downloadHandler.text);
					callback?.Invoke(new ActionStatus(www));
				}
				else
				{
					Debug.Log("Trello API: Feedback post complete!");
					yield return null;

					var id = JsonUtility.FromJson<CardCreateResponce>(www.downloadHandler.text).id;

					List<UnityWebRequestAsyncOperation> attachmentJobs = new List<UnityWebRequestAsyncOperation>();

					if (attachments != null)
					{
						foreach (var attachment in attachments)
						{
							try
							{
								var fileInfo = new System.IO.FileInfo(attachment);

								if (!fileInfo.Exists)
								{
									Debug.LogWarning("Trello API: Attempted to attach a file that does not exist (" + attachment + ")");
									continue;
								}

								byte[] data = System.IO.File.ReadAllBytes(fileInfo.FullName);

								form = new WWWForm();
								form.AddField("key", apiKey);
								form.AddField("token", token);
								form.AddField("name", fileInfo.Name.Replace(fileInfo.Extension, ""));
								form.AddField("mimeType", "");
								form.AddBinaryData("file", data, fileInfo.Name);

								www = UnityWebRequest.Post(Url.cards + id + "/attachments", form);

								attachmentJobs.Add(www.SendWebRequest());

								if (www.result != UnityWebRequest.Result.Success)
								{
									Debug.LogError("Trello API - Attachment (" + attachment + "): [" + www.responseCode + "] " + www.downloadHandler.text);
								}
							}
							catch(Exception ex)
                            {
								Debug.LogException(ex);
                            }
						}
					}

					foreach(var job in attachmentJobs)
                    {
						yield return job;
                    }

					callback?.Invoke(new ActionStatus());
				}
			}
        }
		
	}
}


#endif