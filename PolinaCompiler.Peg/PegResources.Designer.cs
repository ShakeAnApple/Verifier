﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PolinaCompiler.Peg {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class PegResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal PegResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PolinaCompiler.Peg.PegResources", typeof(PegResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///identifier: &quot;[a-zA-Z_][a-zA-Z_0-9]*&quot;;
        ///regex: &quot;\\\&quot;[^\\\&quot;\\\\]*(?:\\\\.[^\\\&quot;\\\\]*)*\\\&quot;&quot;;
        ///chars: &quot;&apos;[^&apos;]*&apos;&quot;;
        ///check: &apos;&amp;&apos; trivial;
        ///not: &apos;^&apos; trivial;
        ///group: &apos;(&apos; exprsSeq &apos;)&apos;;
        ///
        ///num: &quot;[0-9]+&quot;;
        ///quantorSpec: num | (num &apos;,&apos;) | (num &apos;,&apos; num) | (&apos;,&apos; num);
        ///quantor: &apos;*&apos;|&apos;+&apos;|&apos;?&apos;|(&apos;{&apos; quantorSpec &apos;}&apos;);
        ///
        ///number: trivial quantor;
        ///alternatives: altItem (&apos;|&apos; altItem)+;
        ///altItem: number|trivial;
        ///
        ///trivial: identifier|regex|chars|check|not|group;
        ///
        ///exprsSeq: exprItem+;
        ///exprItem: alternatives|number|trivial;
        ///        /// [rest of string was truncated]&quot;;.
        /// </summary>
        public static string defText {
            get {
                return ResourceManager.GetString("defText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot;?&gt;
        ///&lt;Grammar xmlns:xsd=&quot;http://www.w3.org/2001/XMLSchema&quot; xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot; StartRule=&quot;grammar&quot; SkipRule=&quot;skip&quot; xmlns=&quot;SimplePegGrammar&quot;&gt;
        ///  &lt;Rule Name=&quot;identifier&quot;&gt;
        ///    &lt;Regex Pattern=&quot;[a-zA-Z_][a-zA-Z0-9_]*&quot; /&gt;
        ///  &lt;/Rule&gt;
        ///  &lt;Rule Name=&quot;regex&quot;&gt;
        ///    &lt;Regex Pattern=&quot;\&amp;quot;[^\&amp;quot;\\]*(?:\\.[^\&amp;quot;\\]*)*\&amp;quot;&quot; /&gt;
        ///  &lt;/Rule&gt;
        ///  &lt;Rule Name=&quot;chars&quot;&gt;
        ///    &lt;Regex Pattern=&quot;&apos;[^&apos;]*&apos;&quot; /&gt;
        ///  &lt;/Rule&gt;
        ///  &lt;Rule Name=&quot;check&quot;&gt;
        ///    &lt;Seq&gt;
        ///      &lt;Chars String=&quot;&amp; [rest of string was truncated]&quot;;.
        /// </summary>
        public static string defXml {
            get {
                return ResourceManager.GetString("defXml", resourceCulture);
            }
        }
    }
}
