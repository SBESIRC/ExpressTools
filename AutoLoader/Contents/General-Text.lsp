;;by 天华AI研究中心
;;天华效率工具平台-通用工具-文字表格分类相关工具

;Match Text Content :文字内容刷：将文字内容改成源文字内容
(defun c:THMTC (/ txt1 txt2 ent)
    (princ "\n文字内容刷\n") 
	(setq ent (car (entsel "\n选择源文字：")))
	(setq txt1 (entget ent))
	(setq txt1 (cdr (assoc 1 txt1)))
	(while t
		(setq ent (car (entsel "\n选择目标文字：")))
		(setq ent (entget ent))
		(setq ent
			(subst (cons 1 txt1) (assoc 1 ent) ent)
		);setq
		(entmod ent)
	);while
);defun

;Multiple Scale:批量缩放：以各自的开始点（插入点等）为基准点，比例缩放多个实体
(defun c:THMSC(/ num fac i entname entlist bp)
	(princ "\n批量比例缩放\n")
	(setq oldcmd (getvar "cmdecho"))
	(setvar "cmdecho" 0)
	(setq sset (ssget))
	(setq num (sslength sset))
	(setq fac (getreal "\n输入比例系数:"))
	(setq i 0)
	(while (< i num)
		(progn
			(setq entname (ssname sset i))
			(setq entlist (entget entname))
			(setq bp (cdr (assoc 10 entlist)))
			(command "scale" entname "" bp fac)
			(setq i (1+ i))
		);progn
	);while
	(setvar "cmdecho" oldcmd)
);defun
