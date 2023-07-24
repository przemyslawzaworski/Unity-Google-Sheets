using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;

public class UnityGoogleSheets : MonoBehaviour
{
	public string FileName; // https://developers.google.com/workspace/guides/create-credentials?hl=en#create_credentials_for_a_service_account 
	public string ApplicationName;
	public string SpreadsheetId; // Spreadsheet ID can be extracted from its URL
	public string Range; // https://developers.google.com/sheets/api/guides/concepts?hl=en#expandable-1

	public enum State {Write, Read}
	public State Mode = State.Read;

	void Start()
	{
		SheetsService sheetsService = Authentication(FileName, ApplicationName);
		if (Mode == State.Read)
		{
			Read(sheetsService, SpreadsheetId, Range);
		}
		if (Mode == State.Write)
		{
			IList<IList<System.Object>> values = new List<IList<System.Object>>();
			for (int x = 0; x < 3; x++)
			{
				List<System.Object> row = new List<System.Object>();
				for (int y = 0; y < 2; y++)
				{
					row.Add(x.ToString() + " " + y.ToString());
				}
				values.Add(row);
			}
			Write(sheetsService, SpreadsheetId, Range, values);
		}
	}

	SheetsService Authentication(string json, string applicationName)
	{
		string filepath = Path.Combine(Application.streamingAssetsPath, json);
		FileStream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
		string[] scopes = new string[] { SheetsService.Scope.Spreadsheets };
		GoogleCredential credential = GoogleCredential.FromStream(fileStream).CreateScoped(scopes);
		fileStream.Close();
		SheetsService sheetsService = new SheetsService(new BaseClientService.Initializer()
		{
			HttpClientInitializer = credential,
			ApplicationName = applicationName
		});
		return sheetsService;
	}

	async void Read(SheetsService sheetsService, string spreadsheetId, string range)
	{
		SpreadsheetsResource.ValuesResource.GetRequest request = sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);
		ValueRange response = await request.ExecuteAsync();
		IList<IList<System.Object>> values = response.Values;
		if (values != null)
		{
			for (int i = 0; i < values.Count; i++)
			{
				string row = "";
				for (int j = 0; j < values[i].Count; j++)
				{
					row = row + " " + values[i][j];
				}
				Debug.Log(row);
			}
		}
	}

	async void Write(SheetsService sheetsService, string spreadsheetId, string range, IList<IList<System.Object>> values) 
	{
		List<ValueRange> data = new List<ValueRange>();
		ValueRange valueRange = new ValueRange();
		valueRange.Range = range;
		valueRange.Values = values;
		data.Add(valueRange);
		BatchUpdateValuesRequest request = new BatchUpdateValuesRequest();
		request.ValueInputOption = "USER_ENTERED";
		request.Data = data;
		SpreadsheetsResource.ValuesResource.BatchUpdateRequest update = sheetsService.Spreadsheets.Values.BatchUpdate(request, spreadsheetId);
		await update.ExecuteAsync();
	}
}