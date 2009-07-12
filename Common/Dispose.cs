/*
 * File:    Dispose.cs
 * Created: 14.01.2003
 * Author:  Igor Tkachev
 *          mailto:it@rsdn.ru, mailto:igor@tkachev.com
 * 
 * Description: 
 * 
 *     This implements IDisposable interface to classes 
 *     that contain unmanaged objects and resources.
 * 
 * Changed by:
 * 
 * 1.
 *
 */

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Rsdn.Framework.Common
{
	/// <summary>
	/// Is applied to any members that should be disposed automatically. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class UsingAttribute: Attribute 
	{
	}


	/// <summary>
	/// Base class for classes that need to be disposed.
	/// </summary>
	public abstract class DisposableObject: IDisposable
	{
		bool disposed;

		/// <summary>
		/// Clean up all resources.
		/// </summary>
		~DisposableObject()
		{
			if (!disposed) 
			{
				try 
				{
					disposed = true;
					Dispose(false);
				} 
				catch
				{
				}
			}
		}

		/// <summary>
		/// Clean up all resources and delete object from the finalization queue.
		/// </summary>
		public void Dispose()
		{
			if (!disposed) 
			{
				try 
				{
					disposed = true;
					Dispose(true);
					GC.SuppressFinalize(this);
				}
				catch
				{
				}
			}
		}

		/// <summary>
		/// Synonym of the Dispose method.
		/// </summary>
		public void Close()
		{
			Dispose();
		}

		/// <summary>
		/// Must be called if object reopen any resources to return the object in the finalization queue.
		/// </summary>
 		protected void Reopen()
		{
			if (disposed)
			{
				disposed = false;
				GC.ReRegisterForFinalize(this);
			}
		}

		/// <summary>
		/// Can be overridden in child classes.
		/// </summary>
		/// <param name="disposing">Equal 'true' if is called from Dispose method, otherwise from destructor</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing) 
			{
				// All child classes should delete managed objects here...
			}
			
			// ... and unmanaged objects and resources here.
			
			DisposeFields(this);
		}

		/// <summary>
		/// Scans all fields of the object and call DisposeObject method if it's needed.
		/// This method can be used outside of the class.
		/// </summary>
		/// <param name="obj">Disposing object.</param>
		static public void DisposeFields(object obj)
		{
			// Get list of all public, non-public and instance members.
			//
			var fields = 
				obj.GetType().GetFields
					(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);

			foreach (var field in fields)
			{
				// Check 'Using' attribute.
				//
				if (Attribute.IsDefined(field,typeof(UsingAttribute))) 
				{
					var member = field.GetValue(obj);
					DisposeObject(member);
				}
			}
		}

		/// <summary>
		/// Disposes a member in appropriate way.
		/// </summary>
		/// <param name="member">Disposing member.</param>
		static protected void DisposeObject(object member)
		{
			if (member != null)
			{
				// The member has the IDisposable interface.
				//
				if (member is IDisposable)
				{
					((IDisposable)member).Dispose();
				}
				// The member is a COM object.
				//
				else if (member.GetType().IsCOMObject)
				{
					Marshal.ReleaseComObject(member);
				}
				// The member is a list.
				//
				else if (member is IList)
				{
					var list = member as IList;

					// Call DisposeObject for all items.
					//
					foreach (var o in list)
					{
						DisposeObject(o);
					}
				}
			}
		}
	}


	/// <summary>
	/// Base class for MBR classes that need to be disposed.
	/// </summary>
	public abstract class DisposableMbrObject: MarshalByRefObject, IDisposable
	{
		bool disposed;

		/// <summary>
		/// Clean up all resources.
		/// </summary>
		~DisposableMbrObject()
		{
			if (!disposed) 
			{
				try 
				{
					disposed = true;
					Dispose(false);
				} 
				catch
				{
				}
			}
		}

		/// <summary>
		/// Clean up all resources and delete object from the finalization queue.
		/// </summary>
		public void Dispose()
		{
			if (!disposed) 
			{
				try 
				{
					disposed = true;
					Dispose(true);
					GC.SuppressFinalize(this);
				}
				catch
				{
				}
			}
		}

		/// <summary>
		/// Synonym of the Dispose method.
		/// </summary>
		public void Close()
		{
			Dispose();
		}

		/// <summary>
		/// Must be called if object reopen any resources to return the object in the finalization queue.
		/// </summary>
		protected void Reopen()
		{
			if (disposed)
			{
				disposed = false;
				GC.ReRegisterForFinalize(this);
			}
		}

		/// <summary>
		/// Can be overridden in child classes.
		/// </summary>
		/// <param name="disposing">Equal 'true' if is called from Dispose method, otherwise from destructor</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing) 
			{
				// All child classes should delete managed objects here...
			}
			
			// ... and unmanaged objects and resources here.
			
			DisposableObject.DisposeFields(this);
		}
	}
}
