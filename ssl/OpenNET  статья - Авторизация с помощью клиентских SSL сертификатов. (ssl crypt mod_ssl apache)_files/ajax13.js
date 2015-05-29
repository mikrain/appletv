var lines_per_page = 10;
var text_type=GetCookie('ajaxmode');
var old_text_type=text_type;

function numsort(a,b){
	return a-b;
}

function sethtm(id, text){
	var item=document.getElementById(id);
	if (item){
		item.innerHTML=text;
	}
}

function gethtm(id){
	var item=document.getElementById(id);
	if (item){
		return item.innerHTML;
	}
	return null;
}

function rs_init_object(){
	var A;
	try {
		A=new ActiveXObject("Msxml2.XMLHTTP");
	} catch (e) {
		try {
			A=new ActiveXObject("Microsoft.XMLHTTP");
		} catch (oc) {
			A=null;
		}
	}
	if(!A && typeof XMLHttpRequest != "undefined")
		A = new XMLHttpRequest();
	return A;
}

function x_get_thread(){
	var i, x, n;
	var url = "/cgi-bin/openforum/ajax2.cgi?rs=get_thread", a = x_get_thread.arguments;
	for (i = 0; i < a.length-1; i++) 
		url = url + "&rsargs=" + escape(a[i]);
	url = url.replace( /[+]/g, '%2B');
	x = rs_init_object();
	x.open("GET", url, true);
	x.onreadystatechange = function() {
	if (x.readyState != 4) 
		return;
	var status;
	var data;
	status = x.responseText.charAt(0);
	data = x.responseText.substring(2);
	if (status == "-") 
		alert("Error: " + callback_n);
	else  
		a[a.length-1](data, a);
	}
	x.send(null);
}

var block_list = new Object();
var block_thread = new Object();
var text_list = new Object();
var text_thread = new Object();
var wait_buffer = new Object();
var last_show_list= new Object();
var last_show_msg= new Object();
var map_show_msg= new Object();
var current_mode=0;

function do_show_thread_cb(text, arg_arr){
	var om= arg_arr[0];
	var omm= arg_arr[1];
	var forum= arg_arr[2];
  
	var header =  '<table style="border: 1px solid #B0B190; width: 100%"><tr><td>';
	var footer = '<td></tr></table>';

	if (wait_buffer[om] && wait_buffer[om][omm]){
		sethtm('t'+om+'_'+omm, wait_buffer[om][omm]);
		wait_buffer[om][omm]=null;
	}
	if (omm != '-'){
		if (document.getElementById('actshow_' + om + '_' + omm)){
			sethtm('actshow_' + om + '_' + omm, header + text + footer);
		} else if (document.getElementById('t' + om + '_' + omm)) {
		    if (typeof hidden_msg != "undefined" && current_mode == 0){
			document.getElementById('t' + om + '_' + omm).innerHTML += '<div id=actshow_'+ om + '_' + omm + ' style="position: relative; left: 0px; width: 100%">'+  text + '</div>';
		    } else {
			document.getElementById('t' + om + '_' + omm).innerHTML += '<div id=actshow_'+ om + '_' + omm + ' style="position: relative; left: 0px; width: 100%">'+ header + text + footer + '<table width="100%"><tr><td style="font-size: 60%"><a href="/cgi-bin/openforum/vsluhboard.cgi?az=to_moderator&forum=vsluhforumID'+forum+'&om='+om+'&omm='+omm+'">сообщить модератору</a>' 
			+ '&nbsp;<span class="vt_d2" id="vt_2_'+forum+'_'+om+'_'+omm+'"><a href="#" onclick="return o_vote(\'2_'+forum+'_'+om+'_'+omm+'\',1);" title="Нравится"><span class="vt_p">+</span></a>/<a href="#" onclick="return o_vote(\'2_'+forum+'_'+om+'_'+omm+'\',-1);" title="Не нравится"><span class="vt_m">-</span></a></span>' 
			+ '</td><td style="text-align: right; font-size: 11pt"><a href="/cgi-bin/openforum/vsluhboard.cgi?quote=not_empty&az=post&forum=vsluhforumID'+forum+'&om='+om+'&omm='+omm+'">ответить</a></td></tr></table></div>';
		    }
		}
	} else {
		header = '<br>' + header;
		var re1 = new RegExp('<a href="#([0-9]+)">','g');
		text = text.replace(re1, '<a name=t"' + om + '_$1"></a><a href="/openforum/vsluhforumID'+ forum +'/'+om+'.html#'+"$1"+'" onclick="return do_show_thread(' + om + ", $1, " + forum + ');">');
		var re2 = new RegExp('<li id="t([0-9]+)">','g');
		text = text.replace(re2,'<li id="t' + om + "_$1" + '">');
		if (typeof s_n_repl != "undefined"){
			var re3 = new RegExp('<scr'+'ipt>s_n\\(([0-9]+),\'(vsluhforumID[0-9]+)\',\'([^\']+)\'\\)</scr'+'ipt>','g');
			text = text.replace(re3, s_n_repl);
		}
		// разбиение на линии
		block_thread[om] = new Array;
		block_list[om] = new Array;
		var lines = new Array;
		var re4 = new RegExp('(<li id="t[0-9]+_([0-9]+)">[^\0]+?)</(li|ul)>','g');
		var res;
		var omm_cnt=1;
		block_thread[om][0]='0';
		block_list[om][0]='0';
		while ((res = re4.exec(text)) != null){
			block_thread[om][omm_cnt]=res[2];
			block_list[om][omm_cnt]=res[2];
			lines[res[2]]=res[1] + '</li>';
			omm_cnt++;
		}
		block_list[om].sort(numsort);
		if (omm_cnt > 1){
			var block_data = new Array();
			for( var I=1; I <= 1 + (omm_cnt - 1)/lines_per_page; I++){
				block_data[I-1] = '<a href="#" id="blh_' + om + '_' + I + '" onclick="return do_show_block('+ om + ',' + forum + ',' + I + ');">' + I + '</a>';
			}
			var inv_title;
			if (text_type == 0){
				inv_title="дате";
			} else {
				inv_title="нитям";
			}
//			header += '<a name="t'+om+'_0"></a><ul><div style="padding: 0" id=t' + om + '_0><table style="width: 100%; border-collapse: collapse; border:0"><tr><td><li><a href="#" onclick="return do_show_thread(' + om + ", 0, " + forum + ');" style="font-size: 11pt">Основное сообщение</a></li></td><td style="text-align: right; font-size: 60%">Сортировать по: [<a href="#" id="view_type'+om+'" onclick="return do_invert_type(\''+forum+'\', '+om+');>'+ inv_title +'</a>] Открыть частями: [ ' + block_data.join(' | ') + ' ]</td></tr></table></div>';
			header += '<ul><div style="padding: 0" id=t' + om + '_0><table style="width: 100%; border-collapse: collapse; border:0"><tr><td><li><a href="/openforum/vsluhforumID'+ forum +'/'+om+'.html#0" onclick="return do_show_thread(' + om + ", 0, " + forum + ');" style="font-size: 11pt">Основное сообщение</a></li></td><td style="text-align: right; font-size: 60%">Сортировать по: [<a href="#" id="view_type'+om+'" onclick="return do_invert_type(\''+forum+'\', '+om+');">'+ inv_title +'</a>] Открыть частями: [ ' + block_data.join(' | ') + ' ]</td></tr></table></div>';
			footer = '<table width="100%"><td style="text-align: right; font-size: 60%"><a href="/cgi-bin/openforum/bv.cgi?act_add_thread=1&new_forum=vsluhforumID'+forum+'&new_om='+om+'">Отслеживать</a> | Открыть частями: [ ' + block_data.join(' | ') + ' ]</td></tr></table>' + footer;
			footer = footer.replace(/id="blh_/g, 'id="blf_'); //'
			text_list[om] = header + lines.join("") + footer;
		} else {
			footer += '<table width="100%"><tr><td><a href="/cgi-bin/openforum/vsluhboard.cgi?az=to_moderator&forum=vsluhforumID'+forum+'&om='+om+'&omm=0">Сообщить модератору</a> | <a href="/cgi-bin/openforum/bv.cgi?act_add_thread=1&new_forum=vsluhforumID'+forum+'&new_om='+om+'">отслеживать</a></td><td align=right><a href="/cgi-bin/openforum/vsluhboard.cgi?quote=not_empty&az=post&forum=vsluhforumID'+forum+'&om='+om+'&omm=0">ответить</a></td></tr></table>';
			text_list[om] = header + text + footer;
		}

		text_thread[om] = header + text + footer;
		if (text_type == 0){
			sethtm('ln' + om, text_thread[om]);
		}else{
			sethtm('ln' + om, text_list[om]);
		}
	}   
}

function do_show_multithread_cb(text, arg_arr) {
	var om= arg_arr[0];
	var omm= arg_arr[1];
	var forum= arg_arr[2];

	var re = new RegExp('<!--end:([0-9]+)-->','g');
	var res;
	var last_pos=0;
	while ((res = re.exec(text)) != null){
		var cur_omm=res[1];
		if (cur_omm != ""){
			var one_text=text.slice(last_pos,res.index-1);
			last_pos = re.lastIndex;
			var one_param = [om, cur_omm, forum];
			do_show_thread_cb(one_text, one_param);
		}
	}
}


function do_show_mix(om,forum){
    do_show_thread(om,'-',forum);
    setTimeout("do_show_thread("+om+", 0, "+forum+")",500);
    return false;
}

function do_show_page(om,forum,page){
    if (! map_show_msg[om]){ 
	do_show_thread(om,'-',forum);
    }
    if (page == 0){
	setTimeout("do_show_last("+om+", "+forum+","+10+")",1000);
    } else {
	setTimeout("do_show_block("+om+", "+forum+","+page+")",1000);
    }
    return false;
}

function do_show_page_line(om,forum,page){

    old_text_type=text_type;
    text_type=1;
    do_show_page(om,forum,page);
    setTimeout("text_type=old_text_type;",3000);
    return false; 
}

function do_show_thread0(om,omm,forum) {

    var item = document.getElementById('thd' + om + '_' + omm);
    if (item){
        item.style.display='none';
    }
    do_show_thread(om,omm,forum);
    return false;
}
function do_show_thread(om,omm,forum) {

	var re = new RegExp('Konqueror/[123].[01234]','');
	if (navigator.userAgent && re.exec(navigator.userAgent) != null){
		return true;
	}
	//var re2 = new RegExp('Konqueror','');
	//if (navigator.userAgent && re2.exec(navigator.userAgent) != null){
	//}
	
	if (! map_show_msg[om]){
		map_show_msg[om] = new Array();
	}
	if (last_show_msg[om] != null){
		//vis_hide_msg(om, last_show_msg[om], forum);
		//map_show_msg[om][last_show_msg[om]]=0;
	}
    
	if (map_show_msg[om][omm] == 1){
		vis_hide_msg(om, omm, forum);
		map_show_msg[om][omm]=0;
		last_show_msg[om]=null;//diff
	}else if (omm != '-' && last_show_msg[om] != omm){
		last_show_msg[om]=omm;
		vis_show_msg(om, omm, forum);
		map_show_msg[om][omm]=1;
	} else {
		last_show_msg[om]=null;
	}

	if (omm == '-') {
		if (last_show_list[om] != null){
			vis_hide_thread(om, forum);
			last_show_msg[om]=null;
			last_show_list[om]=null;
		} else {
			vis_show_thread(om, forum);
			last_show_list[om]=0;
		}
	}
        return false;
}
var old_data = new Array();

function do_switch_thread(om,forum) {

	if (! map_show_msg[om]){
		map_show_msg[om] = new Array();
	}
	var ln_item = document.getElementById('ln' + om);
	var old_html=old_data[om];
	if (ln_item){
		old_data[om]=ln_item.innerHTML;
		if (old_html){
			ln_item.innerHTML=old_html;
		}
	}
	if (last_show_list[om] != null){
		last_show_msg[om]=null;
		last_show_list[om]=null;
		sethtm('smode', 'Ajax режим');
		current_mode=0;
	} else {
		current_mode=1;
		if (!old_html){
			vis_show_thread(om, forum);
			setTimeout("do_show_new2("+om+", "+forum+")",1000); 
		}
		sethtm('smode', 'Табличный режим');
		last_show_list[om]=0;
	}
}


function do_show_block(om, forum, num){

	var first_omm = null;
	if (last_show_list[om] != null){
		if (last_show_list[om] == num){
			first_omm = 0;
			vis_hide_block(om, forum, num);
			vis_hide_all(om, forum);
			last_show_msg[om]=null;
			last_show_list[om]=0;
		}else if (last_show_list[om] > 0){
			vis_hide_block(om, forum, last_show_list[om]);
			vis_hide_all(om, forum);
			last_show_msg[om]=null;
			first_omm = vis_show_block(om, forum, num);
			last_show_list[om]=num;
		}else if (last_show_list[om] == 0){
			first_omm = vis_show_block(om, forum, num);
			last_show_list[om]=num;
		}
	}
	if (first_omm != null && navigator.userAgent && navigator.userAgent.indexOf( "MSIE" ) == -1){
		return true;
	}else{
		return false;
	}
}

function do_invert_type(forum, om){

	if (text_type == 0){
		text_type = 1;
	} else {
		text_type = 0;
	}
	document.cookie='ajaxmode=' + text_type + '; path=/cgi-bin/openforum; expires=Thu, 19-May-2015 10:38:47 GMT';
	for (var open_om in last_show_list){
		if (open_om > 0){
			var displ= document.getElementById('ln' + open_om);
			if (displ){
				if (last_show_list[open_om] > 0){
					vis_hide_block(om, forum, last_show_list[open_om]);
				}
				vis_hide_all(open_om, forum);
				last_show_msg[open_om]=null;
				last_show_list[om]=0;
				if (text_type == 0){
					text_list[open_om] = displ.innerHTML;
					displ.innerHTML = text_thread[open_om];
				}else{
					text_thread[open_om]=displ.innerHTML;
					displ.innerHTML = text_list[open_om];
				}
			}
			var inv_item = document.getElementById('view_type' + open_om);
			if (inv_item){
				if (text_type == 1){
					inv_item.innerHTML="нитям";
				}else{
					inv_item.innerHTML="дате";
				}
			}
		}
	}
    return false;
}

function vis_hide_msg(om,omm,forum) {

	var actshow_item = document.getElementById('actshow_' + om + '_' + omm);
	if (actshow_item){
		actshow_item.style.display='none';
	}
	var t_item = document.getElementById('t' + om + '_' + omm);
	if (t_item){
		t_item.style.color = '#999999';
	}
}

function vis_show_msg(om,omm,forum) {

	var actshow_item = document.getElementById('actshow_' + om + '_' + omm);
	if (actshow_item){
		actshow_item.style.display='block';
	} else {
		if (! wait_buffer[om]){
			wait_buffer[om] = new Array();
		}
	 	wait_buffer[om][omm]=gethtm('t'+om+'_'+omm);
		sethtm('t'+om+'_'+omm, 'загрузка....');
		x_get_thread(om, omm, forum, do_show_thread_cb);
	}
}

function vis_invert_msg(om,omm,forum) {

    var actshow_item = document.getElementById('actshow_' + om + '_' + omm);
    if (actshow_item){
    	if (actshow_item.style.display == 'none'){
	    actshow_item.style.display='block';
	} else {
	    actshow_item.style.display='none';
	}
    }
    return false;
}

function vis_hide_thread(om, forum) {

	if (last_show_list[om] && last_show_list[om] > 0){
		vis_hide_block(om, forum, last_show_list[om]);
	}
	vis_hide_all(om, forum);
	var ln_item = document.getElementById('ln' + om);
	if (ln_item){
		ln_item.style.display='none';
	}
	sethtm('swc' + om, '+');
	sethtm('swt' + om, '+');
}

function vis_show_thread(om,forum) {
	var ln_item = document.getElementById('ln' + om);
	if (ln_item.style.display == 'none'){
		ln_item.style.display='block';
	} else {
		ln_item.innerHTML='загрузка....';
		x_get_thread(om, '-', forum, do_show_thread_cb);
	}
	sethtm('swc' + om, '-');
	sethtm('swt' + om, '-');
}

function vis_hide_block(om, forum, num) {

	set_block_info(om, num, 'blue', '' + num, 0);
}

function vis_show_block(om, forum, num) {

	var first_omm = null;
	var cur_block = block_thread[om];
	if (text_type == 1){
		cur_block = block_list[om];
	}
	var min_pos_num = (0 + num - 1) * lines_per_page;
	var max_pos_num = (0 + num) * lines_per_page;
	var get_list = new Array();
	for (var pos = min_pos_num; pos < max_pos_num; pos++){
		if (cur_block[pos]){
			if (first_omm == null){
				first_omm = cur_block[pos];
			}
			var actshow_item = document.getElementById('actshow_' + om + '_' + cur_block[pos]);
			if (actshow_item){
				actshow_item.style.display='block';
			} else if (document.getElementById('t' + om + '_' + cur_block[pos])) {
				get_list.push(cur_block[pos]);
				//vis_show_msg(om,cur_block[pos],forum);
			}
		}
		map_show_msg[om][cur_block[pos]]=1;
	}
	if (get_list.length > 0){
	    if (get_list.length == 1){
		x_get_thread(om, get_list, forum, do_show_thread_cb);
	    } else {
		x_get_thread(om, get_list.join('.'), forum, do_show_multithread_cb);
		
		if (cur_block[max_pos_num]){
		    var item=document.getElementById('t'+om+'_'+cur_block[max_pos_num]);
		    if (item){
			item.innerHTML =  item.innerHTML+'<div style="width: 60px; padding: 0; float: right; text-align: right;font-size: 75%;"> <a href="#t'+om+'_'+cur_block[max_pos_num]+'" onclick="return do_show_block('+ om + ',' + forum + ',' + (num+1) + ');">'+ (num+1) +' часть</a> </div>';
		    }
		}
	    }
	}

	set_block_info(om, num, 'red', '&gt;' + num + '&lt;', first_omm);
	return first_omm;
}

function vis_hide_all(om, forum) {

	if (! map_show_msg[om]){ return;}

	for (var cur_omm=0; cur_omm < map_show_msg[om].length; cur_omm++){
		if (map_show_msg[om][cur_omm] && map_show_msg[om][cur_omm] > 0){
			var actshow_item = document.getElementById('actshow_' + om + '_' + cur_omm);
			if (actshow_item){
				actshow_item.style.display='none';
			}
			var t_item = document.getElementById('t' + om + '_' + cur_omm);
			if (t_item){
				t_item.style.color = '#999999';
			}
			map_show_msg[om][cur_omm] = 0;
		}
	}
}

function set_block_info(om, num, color, text, first_omm) {
	var blh_item = document.getElementById('blh_' + om + '_' + num);
	var blf_item = document.getElementById('blf_' + om + '_' + num);
	if (blh_item && blf_item){
		blh_item.style.color=color;
		blh_item.innerHTML=text;
		blh_item.setAttribute("HREF", '#t'+om+'_'+first_omm);
		blf_item.style.color=color;
		blf_item.innerHTML=text;
		blf_item.setAttribute("HREF", '#t'+om+'_'+first_omm);
	}
}

function GetCookie (name) {
    var arg = name + "=";
    var alen = arg.length;
    var dc = document.cookie;
    var clen = dc.length;
    var endstr = 0;
    var i = 0;
    while (i < clen) {
	var j = i + alen;
	if (dc.substring(i, j) == arg){
	    endstr = dc.indexOf (";", j);
	    if (endstr == -1){
		endstr = clen;
	    }
	    return parseInt(unescape(dc.substring(j, endstr)));
	}
	i = dc.indexOf(" ", i) + 1;
	if (i == 0) { break; }
    }
    return 0; // null
}
function s_n_a(msg_time, cur_user_name) {
    if (typeof s_n_epoch != "undefined"){
	s_n_epoch(msg_time,cur_user_name);
    }
    return 0;
}

var load_flag=0;

function open_block(om, forum, hidden_list, cookflag){

    if (cookflag == 0){
	if (text_type == 0){
    	    text_type=1;
	} else {
	    text_type=0;
	}
	document.cookie='ajaxmode=' + text_type + '; path=/opennews/; expires=Thu, 19-May-2015 10:38:47 GMT';
    }

    if (text_type == 1){
        sethtm('openlnk', 'Свернуть все');
    } else {
        sethtm('openlnk', 'Показать все');
    }

    if (! map_show_msg[om]){
    	map_show_msg[om] = new Array();
    }
    for (var cur_num=0; cur_num < hidden_list.length; cur_num++){

        var item = document.getElementById('thd' + om + '_' + hidden_list[cur_num]);

	if (load_flag != 0){
	    do_show_thread(om, hidden_list[cur_num],forum);
	    if (item){item.style.display='block';}
	} else {
	    //sethtm('t' + om + '_' + hidden_list[cur_num], ' ');
	    if (item){item.style.display='none';}
	}
	map_show_msg[om][hidden_list[cur_num]]=text_type;
    }

    if (load_flag == 0){
	if (hidden_list.length == 2){
	    x_get_thread(om, hidden_list[1], forum, do_show_thread_cb);
	} else if (hidden_list.length > 2) {
	    hidden_list.shift();
	    x_get_thread(om, hidden_list.join('.'), forum, do_show_multithread_cb);
	}
	load_flag=1;
    }

    return false;
}

function open_comments(om, forum, idx_num){
    if (typeof hidden_idx != "undefined" && typeof hidden_msg != "undefined"){
	load_flag=0; 
	var tmp_arr = Array('0');
	open_block(om, forum, tmp_arr.concat(hidden_msg.slice(hidden_idx[idx_num],hidden_idx[idx_num+1])), 1);
    }
    return false;
}

function open_comments2(om, forum, idx_num){
    if (typeof hidden_idx != "undefined" && typeof hidden_msg != "undefined"){
	var tmp_arr = Array();
	do_show_arr(om, forum, tmp_arr.concat(hidden_msg.slice(hidden_idx[idx_num],hidden_idx[idx_num+1])));
    }
    return false;
}

function do_show_new(om,forum){
    do_show_thread(om,'-',forum);
    setTimeout("do_show_new2("+om+", "+forum+")",1000);
    return false;
}

function do_show_new2(om, forum){

    var re = new RegExp('<li id="t[0-9]+_([0-9]+)">[^\0]+?(......)</li>','g');
    var res;
    var cnt = 0;
    var new_list = new Array();
    while ((res = re.exec(text_list[om])) != null){
        if (res[2] == '"!*!">'){
		new_list.push(res[1]);
		cnt++;
	}
    }
    if (cnt == 0){
	new_list = block_list[om].slice(-10);
    }
    return do_show_arr(om, forum, new_list);
}

function do_show_last(om, forum, last_num){

    if (block_list[om]){
	var new_list = block_list[om].slice(-last_num);
	do_show_arr(om, forum, new_list);
    }
    return false;
}

function do_show_arr(om, forum, new_list){
    var get_flag=0;
    if (! map_show_msg[om]){
	map_show_msg[om] = new Array();
    } else {
	get_flag=1;
    }

    for (i = 0; i < new_list.length; i++){
	var omm = new_list[i];
	if (map_show_msg[om][omm] == 1){
		map_show_msg[om][omm]=0;
		vis_hide_msg(om, omm, forum);
	} else if (map_show_msg[om][omm] == null){
		map_show_msg[om][omm]=1;
		get_flag=0;
	} else {
		map_show_msg[om][omm]=1;
		vis_invert_msg(om,omm,forum);
	}
    }
    if (get_flag == 0){
	if (new_list.length == 1){
    	    x_get_thread(om, new_list[0], forum, do_show_thread_cb);
	} else if (new_list.length > 1) {
    	    x_get_thread(om, new_list.join('.'), forum, do_show_multithread_cb);
	}
    }
    return false;
}

