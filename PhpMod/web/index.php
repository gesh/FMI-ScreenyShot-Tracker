<?php

namespace Piss;

use Doctrine\ORM\EntityManager;
use Exception;
use Imagine\Gd\Imagine;
use Imagine\Image\Box;
use Piss\Entities\Users;

$dbg = false;
ini_set('display_errors', $dbg);
ini_set('display_startup_errors', $dbg);
error_reporting($dbg ? -1 : 0);

if (!defined('PHP_VERSION_ID') || PHP_VERSION_ID < 50600) {
    http_response_code(500) && die("old php version");
}

\date_default_timezone_set("Europe/Sofia");
require_once __DIR__ . '/../vendor/autoload.php';


/* @var $em EntityManager */
$em = EmCreator::getEm();
$email = $_POST['email'];// = 'admin@test.com';
$pass = $_POST['pass'] ;//= 'rasmuslerdorf';
//$_POST['action'] = 'screen';
//$_POST['delEmail'] = 'al';
//$_POST['newEmail'] = 'al';
//$_POST['newPass'] = 'wd';

$auth = getAuth($em, $email, $pass);

$action = isset($_POST['action']) ? $_POST['action'] : 'nothing';
if ($action == 'screen') {
    $port = isset($_POST['port']) ? (int) $_POST['port'] : 12345;
    $quality = isset($_POST['quality']) ? (int) $_POST['quality'] : 20;
    $res = isset($_POST['resolution']) ? (string) $_POST['resolution'] : '1024x768';
    return getScreenshot($port, $quality, $res);
} else if ($action == 'createuser') {
    if (!isset($_POST['newEmail']) || !isset($_POST['newPass'])) {
        throw new Exception('missing email');
    }
    createUser($em, (string) $_POST['newEmail'], (string) $_POST['newPass']);
    echo 1;
} else if ($action == 'removeuser') {
    if (!isset($_POST['delEmail'])) {
        throw new Exception('missing email');
    }
    removeUser($em, (string) $_POST['delEmail']);
    echo 1;
} else {
    throw new Exception('no action');
}

function getAuth($em, $email, $pass) {
    $getUserQ = $em->createQuery('select u.pass from Piss\\Entities\\Users u where u.email =:em');
    $getUserQ->setParameter('em', $email);
    try {
        $hash = $getUserQ->getSingleScalarResult();
    } catch (Exception $ex) {
        throw new Exception('email not found');
    }
    if (!password_verify($pass, $hash)) {
        throw new Exception('invalid pass');
    }
    return true;
}

function getScreenshot($port, $quality, $resolution) {
    if ($quality > 100 || $quality < 0) {
        throw new Exception;
    }
    $xy = explode("x", $resolution, 2);
    if (count($xy) !== 2) {
        $xy = [1024, 768];
    }
    if ($xy[0] > 2000 || $xy[1] > 2000 || $xy[0] < 1 || $xy[1] < 1) {
        throw new Exception;
    }
    header('Content-Type: application/json');
    $resp = file_get_contents("http://127.0.0.1:{$port}/PISS_java_mod/service/screenshot?quality={$quality}");
    //$resp = json_encode(["ImageData" => base64_encode(file_get_contents("http://www.gearhack.com/images/GearHack%20Banner.png"))]);
    $respObj = json_decode($resp);
    if ($respObj === null || !isset($respObj->ImageData)) {
        throw new Exception;
    }
    $img = new Imagine;
    $toRes = $img->load(base64_decode($respObj->ImageData));

    $toRes->resize(new Box((int) $xy[0], (int) $xy[1]));
    $gdImg = $toRes->getGdResource();
    ob_start();
    imagejpeg($gdImg, null, $quality);
    $imgRes = ob_get_contents();
    ob_end_clean();
    $respObj->ImageData = base64_encode($imgRes);
    echo json_encode($respObj);
}

function createUser($em, $email, $pass) {
    removeUser($em, $email);
    $usr = new Users();
    $usr->setEmail($email);
    $usr->setPass(password_hash($pass, PASSWORD_BCRYPT));
    $em->persist($usr);
    $em->flush();
}

function removeUser($em, $email) {
    $delUserQ = $em->createQuery('delete from Piss\\Entities\\Users u where u.email =:em');
    $delUserQ->setMaxResults(1);
    $delUserQ->setParameter('em', $email);
    $delUserQ->execute();
}
