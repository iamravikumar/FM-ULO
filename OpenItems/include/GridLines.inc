
    <script type="text/javascript" >

      function document_onmouseout()
      {
      if(window.event.srcElement == null )	return;
      if (window.event.srcElement != null)
      if (window.event.srcElement.parentElement != null)
      {
      window.event.srcElement.parentElement.style.cursor = "";
      if (window.event.srcElement.parentElement.className == "TDeven" || window.event.srcElement.parentElement.className == "TDborder")
      window.event.srcElement.parentElement.style.backgroundColor = "#F0EFE8";
      if (window.event.srcElement.parentElement.className == "TDodd")
      window.event.srcElement.parentElement.style.backgroundColor = "";      
      }
      }

      function document_onmouseover()
      {
      if(window.event.srcElement == null )	return;
      if (window.event.srcElement != null)
      if (window.event.srcElement.parentElement != null)
      if (window.event.srcElement.parentElement.className == "TDeven" || window.event.srcElement.parentElement.className == "TDodd"
      || window.event.srcElement.parentElement.className == "TDborder")
      {
      window.event.srcElement.parentElement.style.backgroundColor = "#cccccc";
      window.event.srcElement.parentElement.style.cursor = "hand";
      }
      }

    </script>
    <script LANGUAGE="javascript" FOR="document" EVENT="onmouseover">
    <!--
	    document_onmouseover()
    // -->
    </script>

    <script LANGUAGE="javascript" FOR="document" EVENT="onmouseout">
    <!--
	    document_onmouseout()
    // -->
    </script>

