﻿using System;
using System.Web;
using SimpleStack.Enums;
using SimpleStack.Interfaces;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using SimpleStack.Extensions;
using System.Net;

namespace SimpleStack.Handlers
{
	public class SimpleStackHttpHandlerFactory
	{
		static readonly List<string> WebHostRootFileNames = new List<string>();
		static private readonly string WebHostPhysicalPath = null;
		static private readonly string DefaultRootFileName = null;
		static private string ApplicationBaseUrl = null;
		static private readonly ISimpleStackHttpHandler DefaultHttpHandler = null;
		static private readonly RedirectHttpHandler NonRootModeDefaultHttpHandler = null;
		static private readonly ISimpleStackHttpHandler ForbiddenHttpHandler = null;
		static private readonly ISimpleStackHttpHandler NotFoundHttpHandler = null;
		static private readonly ISimpleStackHttpHandler StaticFileHandler = new StaticFileHandler();
		private static readonly bool IsIntegratedPipeline = false;
		private static readonly bool ServeDefaultHandler = false;
		private static readonly bool AutoRedirectsDirs = false;
		private static Func<IHttpRequest, ISimpleStackHttpHandler>[] RawHttpHandlers;

		[ThreadStatic]
		public static string DebugLastHandlerArgs;

//		static SimpleStackHttpHandlerFactory()
//		{
//			//TODO: vdaron : fix IsIntegratedPipeline ??
//			//MONO doesn't implement this property
//			System.Reflection.PropertyInfo pi = null;// typeof(HttpRuntime).GetProperty("UsingIntegratedPipeline");
//			if (pi != null)
//			{
//				IsIntegratedPipeline = (bool)pi.GetGetMethod().Invoke(null, new object[0]);
//			}

//			var config = EndpointHost.Config;
//			if (config == null)
//			{
//				throw new ConfigurationErrorsException(
//					"ServiceStack: AppHost does not exist or has not been initialized. "
//					+ "Make sure you have created an AppHost and started it with 'new AppHost().Init();' in your Global.asax Application_Start()",
//					new ArgumentNullException("EndpointHost.Config"));
//			}

//			var isAspNetHost = false;// HttpListenerBase.Instance == null || HttpContext.Current != null;
//			WebHostPhysicalPath = config.WebHostPhysicalPath;
//			AutoRedirectsDirs = isAspNetHost && !Env.IsMono;

//			//Apache+mod_mono treats path="servicestack*" as path="*" so takes over root path, so we need to serve matching resources
//			var hostedAtRootPath = config.ServiceStackHandlerFactoryPath == null;

//			//DefaultHttpHandler not supported in IntegratedPipeline mode
////			if (!IsIntegratedPipeline && isAspNetHost && !hostedAtRootPath && !Env.IsMono)
////				DefaultHttpHandler = new DefaultHttpHandler();

//			ServeDefaultHandler = hostedAtRootPath || Env.IsMono;
//			if (ServeDefaultHandler && !String.IsNullOrEmpty(WebHostPhysicalPath))
//			{
//				foreach (var filePath in Directory.GetFiles(WebHostPhysicalPath))
//				{
//					var fileNameLower = Path.GetFileName(filePath).ToLower();
//					if (DefaultRootFileName == null && config.DefaultDocuments.Contains(fileNameLower))
//					{
//						//Can't serve Default.aspx pages when hostedAtRootPath so ignore and allow for next default document
//						if (!(hostedAtRootPath && fileNameLower.EndsWith(".aspx")))
//						{
//							DefaultRootFileName = fileNameLower;
//							((StaticFileHandler)StaticFileHandler).SetDefaultFile(filePath);

//							if (DefaultHttpHandler == null)
//								DefaultHttpHandler = new RedirectHttpHandler { RelativeUrl = DefaultRootFileName };
//						}
//					}
//					WebHostRootFileNames.Add(Path.GetFileName(fileNameLower));
//				}
//				foreach (var dirName in Directory.GetDirectories(WebHostPhysicalPath))
//				{
//					var dirNameLower = Path.GetFileName(dirName).ToLower();
//					WebHostRootFileNames.Add(Path.GetFileName(dirNameLower));
//				}
//			}

//			if (!string.IsNullOrEmpty(config.DefaultRedirectPath))
//				DefaultHttpHandler = new RedirectHttpHandler { RelativeUrl = config.DefaultRedirectPath };

//			if (DefaultHttpHandler == null && !string.IsNullOrEmpty(config.MetadataRedirectPath))
//				DefaultHttpHandler = new RedirectHttpHandler { RelativeUrl = config.MetadataRedirectPath };

//			if (!string.IsNullOrEmpty(config.MetadataRedirectPath))
//				NonRootModeDefaultHttpHandler = new RedirectHttpHandler { RelativeUrl = config.MetadataRedirectPath };

//			if (DefaultHttpHandler == null)
//				DefaultHttpHandler = NotFoundHttpHandler;

//			var defaultRedirectHanlder = DefaultHttpHandler as RedirectHttpHandler;
//			var debugDefaultHandler = defaultRedirectHanlder != null
//				? defaultRedirectHanlder.RelativeUrl
//				: "typeof(DefaultHttpHandler).Name";

//			SetApplicationBaseUrl(config.WebHostUrl);

//			ForbiddenHttpHandler = config.GetHandlerForErrorStatus(HttpStatusCode.Forbidden);
//			if (ForbiddenHttpHandler == null)
//			{
//				ForbiddenHttpHandler = new ForbiddenHttpHandler
//				{
//					IsIntegratedPipeline = IsIntegratedPipeline,
//					WebHostPhysicalPath = WebHostPhysicalPath,
//					WebHostRootFileNames = WebHostRootFileNames,
//					ApplicationBaseUrl = ApplicationBaseUrl,
//					DefaultRootFileName = DefaultRootFileName,
//					DefaultHandler = debugDefaultHandler,
//				};
//			}

//			NotFoundHttpHandler = config.GetHandlerForErrorStatus(HttpStatusCode.NotFound);
//			if (NotFoundHttpHandler == null)
//			{
//				NotFoundHttpHandler = new NotFoundHttpHandler
//				{
//					IsIntegratedPipeline = IsIntegratedPipeline,
//					WebHostPhysicalPath = WebHostPhysicalPath,
//					WebHostRootFileNames = WebHostRootFileNames,
//					ApplicationBaseUrl = ApplicationBaseUrl,
//					DefaultRootFileName = DefaultRootFileName,
//					DefaultHandler = debugDefaultHandler,
//				};
//			}

//			var rawHandlers = config.RawHttpHandlers;
//			//rawHandlers.Add(ReturnRequestInfo);
//			//TODO: vdaron restore MiniProfiler
//			//rawHandlers.Add(MiniProfilerHandler.MatchesRequest);
//			RawHttpHandlers = rawHandlers.ToArray();
//		}

//		// Entry point for ASP.NET
//		public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
//		{
//			DebugLastHandlerArgs = requestType + "|" + url + "|" + pathTranslated;
//			var httpReq = new HttpRequestWrapper(pathTranslated, context.Request);
//			foreach (var rawHttpHandler in RawHttpHandlers)
//			{
//				var reqInfo = rawHttpHandler(httpReq);
//				if (reqInfo != null) return reqInfo;
//			}
//
//			var mode = EndpointHost.Config.ServiceStackHandlerFactoryPath;
//			var pathInfo = context.Request.GetPathInfo();
//
//			//WebDev Server auto requests '/default.aspx' so recorrect path to different default document
//			if (mode == null && (url == "/default.aspx" || url == "/Default.aspx"))
//				pathInfo = "/";
//
//			//Default Request /
//			if (string.IsNullOrEmpty(pathInfo) || pathInfo == "/")
//			{
//				//Exception calling context.Request.Url on Apache+mod_mono
//				var absoluteUrl = Env.IsMono ? url.ToParentPath() : context.Request.GetApplicationUrl();
//				if (ApplicationBaseUrl == null)
//					SetApplicationBaseUrl(absoluteUrl);
//
//				//e.g. CatchAllHandler to Process Markdown files
//				var catchAllHandler = GetCatchAllHandlerIfAny(httpReq.HttpMethod, pathInfo, httpReq.GetPhysicalPath());
//				if (catchAllHandler != null) return catchAllHandler;
//
//				return ServeDefaultHandler ? DefaultHttpHandler : NonRootModeDefaultHttpHandler;
//			}
//
//			if (mode != null && pathInfo.EndsWith(mode))
//			{
//				var requestPath = context.Request.Path.ToLower();
//				if (requestPath == "/" + mode
//					|| requestPath == mode
//					|| requestPath == mode + "/")
//				{
//					if (context.Request.PhysicalPath != WebHostPhysicalPath
//						|| !File.Exists(Path.Combine(context.Request.PhysicalPath, DefaultRootFileName ?? "")))
//					{
//						return new IndexPageHttpHandler();
//					}
//				}
//
//				var okToServe = ShouldAllow(context.Request.FilePath);
//				return okToServe ? DefaultHttpHandler : ForbiddenHttpHandler;
//			}
//
//			return GetHandlerForPathInfo(
//				context.Request.HttpMethod, pathInfo, context.Request.FilePath, pathTranslated)
//				?? NotFoundHttpHandler;
//		}
//
		private static void SetApplicationBaseUrl(string absoluteUrl)
		{
			if (absoluteUrl == null) return;

			ApplicationBaseUrl = EndpointHost.Config.WebHostUrl;

			var defaultRedirectUrl = DefaultHttpHandler as RedirectHttpHandler;
			if (defaultRedirectUrl != null && defaultRedirectUrl.AbsoluteUrl == null)
				defaultRedirectUrl.AbsoluteUrl = ApplicationBaseUrl.CombineWith(
					defaultRedirectUrl.RelativeUrl);

			if (NonRootModeDefaultHttpHandler != null && NonRootModeDefaultHttpHandler.AbsoluteUrl == null)
				NonRootModeDefaultHttpHandler.AbsoluteUrl = ApplicationBaseUrl.CombineWith(
					NonRootModeDefaultHttpHandler.RelativeUrl);
		}

		public static string GetBaseUrl()
		{
			return EndpointHost.Config.WebHostUrl ?? ApplicationBaseUrl;
		}

		public static ISimpleStackHttpHandler GetHandler(IHttpRequest httpReq)
		{
			//foreach (var rawHttpHandler in RawHttpHandlers)
			//{
			//	var reqInfo = rawHttpHandler(httpReq);
			//	if (reqInfo != null) 
			//		return reqInfo;
			//}

			var mode = EndpointHost.Config.SimpleStackHandlerFactoryPath;

			var pathInfo = httpReq.PathInfo;



			//Default Request /
			//if (string.IsNullOrEmpty(pathInfo) || pathInfo == "/")
			//{
			//	if (ApplicationBaseUrl == null)
			//		SetApplicationBaseUrl(httpReq.GetPathUrl());

			//	//e.g. CatchAllHandler to Process Markdown files
			//	var catchAllHandler = GetCatchAllHandlerIfAny(httpReq.HttpMethod, pathInfo, httpReq.GetPhysicalPath());
			//	if (catchAllHandler != null)
			//		return catchAllHandler;

			//	return ServeDefaultHandler ? DefaultHttpHandler : NonRootModeDefaultHttpHandler;
			//}

			//if (mode != null && pathInfo.EndsWith(mode))
			//{
			//	var requestPath = pathInfo;
			//	if (requestPath == "/" + mode
			//		|| requestPath == mode
			//		|| requestPath == mode + "/")
			//	{
			//		//TODO: write test for this
			//		if (httpReq.GetPhysicalPath() != WebHostPhysicalPath
			//			|| !File.Exists(Path.Combine(httpReq.ApplicationFilePath, DefaultRootFileName ?? "")))
			//		{
			//			return new IndexPageHttpHandler();
			//		}
			//	}

			//	var okToServe = ShouldAllow(httpReq.GetPhysicalPath());
			//	return okToServe ? DefaultHttpHandler : ForbiddenHttpHandler;
			//}

			return GetHandlerForPathInfo(httpReq.HttpMethod, pathInfo, pathInfo, httpReq.GetPhysicalPath());// ?? NotFoundHttpHandler;
		}


		/// <summary>
		/// If enabled, just returns the Request Info as it understands
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
//		private static IHttpHandler ReturnRequestInfo(HttpRequest httpReq)
//		{
//			if (EndpointHost.Config.DebugOnlyReturnRequestInfo
//				|| (EndpointHost.DebugMode && httpReq.PathInfo.EndsWith("__requestinfo")))
//			{
//				var reqInfo = RequestInfoHandler.GetRequestInfo(
//					new HttpRequestWrapper(typeof(RequestInfo).Name, httpReq));
//
//				reqInfo.Host = EndpointHost.Config.DebugAspNetHostEnvironment + "_v" + Env.ServiceStackVersion + "_" + EndpointHost.Config.ServiceName;
//				//reqInfo.FactoryUrl = url; //Just RawUrl without QueryString 
//				//reqInfo.FactoryPathTranslated = pathTranslated; //Local path on filesystem
//				reqInfo.PathInfo = httpReq.PathInfo;
//				reqInfo.Path = httpReq.Path;
//				reqInfo.ApplicationPath = httpReq.ApplicationPath;
//
//				return new RequestInfoHandler { RequestInfo = reqInfo };
//			}
//
//			return null;
//		}

		//private static ISimpleStackHttpHandler ReturnRequestInfo(IHttpRequest httpReq)
		//{
		//	if (EndpointHost.Config.DebugOnlyReturnRequestInfo
		//		|| (EndpointHost.DebugMode && httpReq.PathInfo.EndsWith("__requestinfo")))
		//	{
		//		var reqInfo = RequestInfoHandler.GetRequestInfo(httpReq);

		//		reqInfo.Host = EndpointHost.Config.DebugHttpListenerHostEnvironment + "_v" + Env.ServiceStackVersion + "_" + EndpointHost.Config.ServiceName;
		//		reqInfo.PathInfo = httpReq.PathInfo;
		//		reqInfo.Path = httpReq.GetPathUrl();

		//		return new RequestInfoHandler { RequestInfo = reqInfo };
		//	}

		//	return null;
		//}

		// no handler registered 
		// serve the file from the filesystem, restricting to a safelist of extensions
		//private static bool ShouldAllow(string filePath)
		//{
		//	var fileExt = Path.GetExtension(filePath);
		//	if (string.IsNullOrEmpty(fileExt)) return false;
		//	return EndpointHost.Config.AllowFileExtensions.Contains(fileExt.Substring(1));
		//}

		public static ISimpleStackHttpHandler GetHandlerForPathInfo(string httpMethod, string pathInfo, string requestPath, string filePath)
		{
			var pathParts = pathInfo.TrimStart('/').Split('/');
			if (pathParts.Length == 0) 
				return null;

			var restPath = RestHandler.FindMatchingRestPath(httpMethod, pathInfo);
			if (restPath != null)
				return new RestHandler { RestPath = restPath, RequestName = restPath.RequestType.Name };

			return ProcessPredefinedRoutesRequest(httpMethod, pathInfo, filePath);

			//var existingFile = pathParts[0].ToLower();
			//if (WebHostRootFileNames.Contains(existingFile))
			//{
			//	var fileExt = Path.GetExtension(filePath);
			//	var isFileRequest = !string.IsNullOrEmpty(fileExt);

			//	if (!isFileRequest && !AutoRedirectsDirs)
			//	{
			//		//If pathInfo is for Directory try again with redirect including '/' suffix
			//		if (!pathInfo.EndsWith("/"))
			//		{
			//			var appFilePath = filePath.Substring(0, filePath.Length - requestPath.Length);
			//			var redirect = Handlers.StaticFileHandler.DirectoryExists(filePath, appFilePath);
			//			if (redirect)
			//			{
			//				return new RedirectHttpHandler
			//				{
			//					RelativeUrl = pathInfo + "/",
			//				};
			//			}
			//		}
			//	}

			//	//e.g. CatchAllHandler to Process Markdown files
			//	var catchAllHandler = GetCatchAllHandlerIfAny(httpMethod, pathInfo, filePath);
			//	if (catchAllHandler != null) return catchAllHandler;

			//	if (!isFileRequest) return NotFoundHttpHandler;

			//	return ShouldAllow(requestPath) ? StaticFileHandler : ForbiddenHttpHandler;
			//}

			return GetCatchAllHandlerIfAny(httpMethod, pathInfo, filePath);
		}

		private static ISimpleStackHttpHandler GetCatchAllHandlerIfAny(string httpMethod, string pathInfo, string filePath)
		{
			if (EndpointHost.CatchAllHandlers != null)
			{
				foreach (var httpHandlerResolver in EndpointHost.CatchAllHandlers)
				{
					var httpHandler = httpHandlerResolver(httpMethod, pathInfo, filePath);
					if (httpHandler != null)
						return httpHandler;
				}
			}

			return null;
		}
		private static ISimpleStackHttpHandler ProcessPredefinedRoutesRequest(string httpMethod, string pathInfo, string filePath)
		{
			var pathParts = pathInfo.TrimStart('/').Split('/');
			if (pathParts.Length == 0)
				return null;

			if (pathParts.Length == 1)
			{
				//TODO: vdaron enable soap ??
				//if (pathController == "soap11")
				//	return new Soap11MessageSyncReplyHttpHandler();
				//if (pathController == "soap12")
				//	return new Soap12MessageSyncReplyHttpHandler();

				return null;
			}

			var pathController = string.Intern(pathParts[0].ToLower());

			var pathAction = string.Intern(pathParts[1].ToLower());
			var requestName = pathParts.Length > 2 ? pathParts[2] : null;
			var isReply = pathAction == "syncreply" || pathAction == "reply";
			var isOneWay = pathAction == "asynconeway" || pathAction == "oneway";

			List<string> contentTypes;
			if (EndpointHost.ContentTypeFilter.ContentTypeFormats.TryGetValue(pathController, out contentTypes))
			{
				var contentType = contentTypes[0];

				if (isReply)
					return new GenericHandler(contentType, EndpointAttributes.Reply)
					{
						RequestName = requestName
					};
				if (isOneWay)
					return new GenericHandler(contentType, EndpointAttributes.OneWay)
					{
						RequestName = requestName
					};
			}

			return null;
		}

	}
}

