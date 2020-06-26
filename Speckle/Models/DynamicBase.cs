﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Speckle.Models
{

  /// <summary>
  /// Base class implementing a bunch of nice dynamic object methods, like adding and removing props dynamically. Makes c# feel like json.
  /// <para>Orginally adapted from Rick Strahl 🤘</para>
  /// <para>https://weblog.west-wind.com/posts/2012/feb/08/creating-a-dynamic-extensible-c-expando-object</para>
  /// </summary>
  public class DynamicBase : DynamicObject, IDynamicMetaObjectProvider
  {
    /// <summary>
    /// The actual property bag, where dynamically added props are stored.
    /// </summary>
    private Dictionary<string, object> properties = new Dictionary<string, object>();

    public DynamicBase()
    {

    }

    /// <summary>
    /// Gets properties via the dot syntax.
    /// <para><pre>((dynamic)myObject).superProperty;</pre></para>
    /// </summary>
    /// <param name="binder"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
      return (properties.TryGetValue(binder.Name, out result));
    }

    /// <summary>
    /// Sets properties via the dot syntax.
    /// <para><pre>((dynamic)myObject).superProperty = something;</pre></para>
    /// </summary>
    /// <param name="binder"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
      properties[binder.Name] = value;
      return true;
    }

    /// <summary>
    /// Checks if a dynamic propery exists or not
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool HasMember(string key)
    {
      return properties.ContainsKey(key);
    }

    /// <summary>
    /// Checks if a dynamic propery exists or not and has a specific type
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool HasMember<T>(string key)
    {
      return properties.ContainsKey(key) && (T)properties[key] != null ;
    }

    /// <summary>
    /// Safely gets a dynamic property
    /// </summary>
    /// <typeparam name="T">Type of the property</typeparam>
    /// <param name="object">Base object</param>
    /// <param name="key">Name of the property to get</param>
    /// <param name="def">Default value of the property to use if missing</param>
    /// <returns></returns>
    public T GetMemberSafe<T>(string key, T def)
    {
      //if prop does't exist of has the wrong type, re-assign it
      if (!HasMember(key) || properties[key].GetType() != typeof(T))
      {
        properties[key] = def;

      }

      return (T)properties[key];
    }

    /// <summary>
    /// Sets and gets properties using the key accessor pattern. E.g.:
    /// <para><pre>((dynamic)myObject)["superProperty"] = 42;</pre></para>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public object this[string key]
    {
      get
      {
        if (properties.ContainsKey(key))
          return properties[key];
        try
        {
          return GetType().GetProperty(key).GetValue(this);
        }
        catch
        {
          throw;
        }
      }
      set
      {
        if (properties.ContainsKey(key))
        {
          properties[key] = value;
          return;
        }
        var prop = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(p => p.Name == key);
        if (prop == null)
        {
          properties[key] = value;
          return;
        }
        try
        {
          prop.SetValue(this, value);
        }
        catch
        {
          throw;
        }
      }
    }



    /// <summary>
    /// Gets all of the property names on this class, dynamic or not.
    /// </summary>
    /// <returns></returns>
    public override IEnumerable<string> GetDynamicMemberNames()
    {
      var names = new List<string>();
      foreach (var kvp in properties) names.Add(kvp.Key);

      var pinfos = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
      foreach (var pinfo in pinfos) names.Add(pinfo.Name);

      names.Remove("Item"); // TODO: investigate why we get Item out?
      return names;
    }

    /// <summary>
    /// Gets the names of the defined class properties (typed).
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetInstanceMembers()
    {
      var names = new List<string>();
      var pinfos = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
      foreach (var pinfo in pinfos) names.Add(pinfo.Name);

      names.Remove("Item"); // TODO: investigate why we get Item out?
      return names;
    }

    /// <summary>
    /// Gets the dynamically added property names only.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetDynamicMembers()
    {
      foreach (var kvp in properties)
        yield return kvp.Key;
    }

    /// <summary>
    /// Gets & sets the dynamic properties quickly
    /// </summary>
    /// <returns></returns>
    [JsonIgnore]
    public Dictionary<string, object> DynamicProperties
    {
      get
      {
        return properties;
      }
      set
      {
        this.properties = value;
      }
    }

  }

}
