;;by �컪AI�о�����
;;�컪Ч�ʹ���ƽ̨-ͨ�ù���-���ֱ�������ع���

;Match Text Content :��������ˢ�����������ݸĳ�Դ��������
(defun c:THMTC (/ txt1 txt2 ent)
    (princ "\n��������ˢ\n") 
	(setq ent (car (entsel "\nѡ��Դ���֣�")))
	(setq txt1 (entget ent))
	(setq txt1 (cdr (assoc 1 txt1)))
	(while t
		(setq ent (car (entsel "\nѡ��Ŀ�����֣�")))
		(setq ent (entget ent))
		(setq ent
			(subst (cons 1 txt1) (assoc 1 ent) ent)
		);setq
		(entmod ent)
	);while
);defun

;Multiple Scale:�������ţ��Ը��ԵĿ�ʼ�㣨�����ȣ�Ϊ��׼�㣬�������Ŷ��ʵ��
(defun c:THMSC(/ num fac i entname entlist bp)
	(princ "\n������������\n")
	(setq oldcmd (getvar "cmdecho"))
	(setvar "cmdecho" 0)
	(setq sset (ssget))
	(setq num (sslength sset))
	(setq fac (getreal "\n�������ϵ��:"))
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
