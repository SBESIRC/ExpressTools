;;by �컪AI�о�����
;;�컪Ч�ʹ���ƽ̨-ͨ�ù���-��������

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
