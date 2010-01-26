var IMG_BASE = "%URL%";
var IMG_PLUS = "plus.gif";
var IMG_MINUS = "minus.gif";

function toggleExpand(_this) 
{			
	var tab = _this.parentNode; 
	
	var hid = tab.tBodies[1].style.display == "none";
	var imgUrl = "";
							
	if (hid) {
		imgUrl = IMG_BASE + IMG_MINUS;
		tab.tBodies[1].style.display = "block";
	}
	else {
		imgUrl = IMG_BASE + IMG_PLUS;
		tab.tBodies[1].style.display = "none";
	}
	
	var nodes = tab.tBodies[0].rows[0].cells[0].childNodes;
	var img = null;
	
	for (var i = 0; i < nodes.length; i++) {
		var n = nodes[i];
		
		if (n.tagName == "IMG") {				
			img = n;
			break;
		}
	}
	
	if (img != null)
		img.src = imgUrl;
}