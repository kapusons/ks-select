using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Kapusons.Components.Util
{
	// from http://aspnetwebstack.codeplex.com/SourceControl/latest#src/System.Web.Mvc/ExpressionHelper.cs

	/// <summary>
	/// An helper class for working with expressions.
	/// </summary>
	internal static class ExpressionUtil
	{
		//public static string GetModelName(string expression)
		//{
		//	return
		//		string.Equals(expression,"model",StringComparison.OrdinalIgnoreCase)
		//			? string.Empty // If it's exactly "model", then give them an empty string, to replicate the lambda behavior
		//			: expression;
		//}

		/// <summary>
		/// Obtains the model name from an expression.
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		public static string GetModelName(LambdaExpression expression)
		{
			if (expression==null) throw new ArgumentNullException("expression");

			// Split apart the expression string for property/field accessors to create its name
			var nameParts = new Stack<string>();
			Expression part = expression.Body;

			while (part != null)
			{
				if (part.NodeType == ExpressionType.Call)
				{
					MethodCallExpression methodExpression = (MethodCallExpression)part;

					if (!IsSingleArgumentIndexer(methodExpression))
					{
						break;
					}

					nameParts.Push(
						GetIndexerInvocation(
							methodExpression.Arguments.Single(),
							expression.Parameters.ToArray()));

					part = methodExpression.Object;
				}
				else if (part.NodeType == ExpressionType.ArrayIndex)
				{
					BinaryExpression binaryExpression = (BinaryExpression)part;

					nameParts.Push(
						GetIndexerInvocation(
							binaryExpression.Right,
							expression.Parameters.ToArray()));

					part = binaryExpression.Left;
				}
				else if (part.NodeType == ExpressionType.MemberAccess)
				{
					MemberExpression memberExpressionPart = (MemberExpression)part;
					nameParts.Push("." + memberExpressionPart.Member.Name);
					part = memberExpressionPart.Expression;
				}
				else if (part.NodeType == ExpressionType.Parameter)
				{
					// Dev10 Bug #907611
					// When the expression is parameter based (m => m.Something...), we'll push an empty
					// string onto the stack and stop evaluating. The extra empty string makes sure that
					// we don't accidentally cut off too much of m => m.Model.
					nameParts.Push(string.Empty);
					part = null;
				}
				else
				{
					break;
				}
			}

			// If it starts with "model", then strip that away
			if (nameParts.Count > 0 && string.Equals(nameParts.Peek(),".model",StringComparison.OrdinalIgnoreCase))
			{
				nameParts.Pop();
			}

			if (nameParts.Count > 0)
			{
				return nameParts.Aggregate((left,right) => left + right).TrimStart('.');
			}

			return string.Empty;
		}

		private static string GetIndexerInvocation(Expression expression,ParameterExpression[] parameters)
		{
			Expression converted = Expression.Convert(expression,typeof(object));
			ParameterExpression fakeParameter = Expression.Parameter(typeof(object),null);
			Expression<Func<object,object>> lambda = Expression.Lambda<Func<object,object>>(converted,fakeParameter);
			Func<object,object> func;

			try
			{
				//func = CachedExpressionCompiler.Process(lambda);
				func = lambda.Compile();
			}
			catch (InvalidOperationException ex)
			{
				throw new InvalidOperationException(
					string.Format(
						CultureInfo.CurrentCulture,
						//MvcResources.ExpressionHelper_InvalidIndexerExpression,
						"The expression compiler was unable to evaluate the indexer expression '{0}' because it references the model parameter '{1}' which is unavailable.",
						expression,
						parameters[0].Name),
					ex);
			}

			return "[" + Convert.ToString(func(null),CultureInfo.InvariantCulture) + "]";
		}

		internal static bool IsSingleArgumentIndexer(Expression expression)
		{
			MethodCallExpression methodExpression = expression as MethodCallExpression;
			if (methodExpression == null || methodExpression.Arguments.Count != 1)
			{
				return false;
			}

			return methodExpression.Method
				.DeclaringType
				.GetDefaultMembers()
				.OfType<PropertyInfo>()
				.Any(p => p.GetGetMethod() == methodExpression.Method);
		}

		/// <summary>
		/// Gets a list <see cref="PropertyInfo"/> for the properties references by the specified lambda expression.
		/// </summary>
		/// <param name="lambda"></param>
		/// <returns></returns>
		public static IEnumerable<PropertyInfo> GetPropertyList(this LambdaExpression lambda)
		{
			// https://stackoverflow.com/questions/40495725/get-propertyinfo-from-lambda-expression-but-fails-with-int

			var body = lambda.Body;
			if (body.NodeType == ExpressionType.Convert) body = ((UnaryExpression)body).Operand;
			return GetPropertyList(body as MemberExpression);
		}
		private static IEnumerable<PropertyInfo> GetPropertyList(MemberExpression body)
		{
			var result = new List<PropertyInfo>();
			if (body != null && body.NodeType==ExpressionType.MemberAccess)
			{
				if (body.Expression!=null) result.AddRange(GetPropertyList(body.Expression as MemberExpression));
				result.Add(body.Member as PropertyInfo);
			}

			return result;
		}
	}
}
