function toggleExpand(_this) 
{			
	var tab = _this.parentNode; 	
	var hid = tab.tBodies[1].style.display == "none";
	var clsName = "";
							
	if (hid) {
		clsName = "hidden_Minus";
		tab.tBodies[1].style.display = "";
	}
	else {
		clsName = "hidden_Plus";
		tab.tBodies[1].style.display = "none";
	}
	
	var td = tab.tBodies[0].rows[0].cells[0];	
	td.className = clsName;
}